using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.Infrastructure;
using CHAD.Model.RVACModule;
using CHADSOSIEL;

namespace CHAD.Model
{
    public class Simulator
    {
        #region Fields

        private readonly ILoggerFactory _loggerFactory;

        private SimulatorStatus _status;

        #endregion

        #region Constructors

        public Simulator(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        #endregion

        #region Public Interface

        public AgroHydrology AgroHydrology { get; private set; }

        public Climate Climate { get; private set; }

        public Configuration Configuration { get; private set; }

        public void Continue()
        {
            if (Status != SimulatorStatus.OnPaused)
                throw new InvalidOperationException("Simulator is not on pause");

            Status = SimulatorStatus.Run;
        }

        public int CurrentDay { get; private set; }

        public int CurrentSeason { get; private set; }

        public int CurrentSimulation { get; private set; }

        public void Pause()
        {
            if (Status == SimulatorStatus.OnPaused)
                throw new InvalidOperationException("Simulator is already on pause");

            Status = SimulatorStatus.OnPaused;
        }

        public RVAC RVAC { get; private set; }

        public event Action<SimulationResult> SimulationResultObtained;

        public void Start(Configuration configuration)
        {
            if (Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Simulator is already running");

            Configuration = configuration;

            Status = SimulatorStatus.Run;

            Simulate();

            Stop();
        }

        public SimulatorStatus Status
        {
            get => _status;
            private set
            {
                _status = value;
                StatusChanged?.Invoke();
            }
        }

        public event Action StatusChanged;

        public void Stop()
        {
            if (Status == SimulatorStatus.Stopped)
                throw new InvalidOperationException("Simulator is already stopped");

            CurrentSimulation = 0;
            CurrentSeason = 0;
            CurrentDay = 0;

            Status = SimulatorStatus.Stopped;
        }

        #endregion

        #region All other members

        private SosielModel CreateSosielModel(Configuration configuration, List<FieldHistory> fieldHistories)
        {
            var model = new SosielModel
            {
                WaterInAquifire = (double)configuration.Parameters.WaterInAquifer,
                WaterInAquiferMax = (double)configuration.Parameters.WaterInAquiferMax,
                SustainableLevelAquifer = (double)configuration.Parameters.SustainableLevelAquifer
            };

            model.Fields = new List<ChadField>();

            foreach (var fieldHistory in fieldHistories)
            {
                model.Fields.Add(new ChadField
                {
                    Number = fieldHistory.Field.FieldNumber,
                    FieldHistoryCrop = fieldHistory.GetCropNumberSeasons(),
                    FieldHistoryNonCrop = fieldHistory.GetNonCropNumberSeasons()
                });
            }

            return model;
        }

        private void CheckStatus()
        {
            while (Status == SimulatorStatus.OnPaused)
                Thread.Sleep(250);

            if (Status == SimulatorStatus.Stopped)
                Thread.CurrentThread.Abort();
        }

        private string MakeSimulationSession()
        {
            return DateTimeOffset.Now.ToString("yyyy.MM.dd -- HH-mm-ss");
        }

        private void RaiseSimulationResultObtained(SimulationResult simulationResult)
        {
            SimulationResultObtained?.Invoke(simulationResult);
        }

        private void Simulate()
        {
            var simulationSession = MakeSimulationSession();

            for (var simulationNumber = 1;
                simulationNumber <= Configuration.Parameters.NumOfSimulations;
                simulationNumber++)
            {
                var simulationResult = new SimulationResult(simulationSession, Configuration, simulationNumber);

                CheckStatus();
                CurrentSimulation = simulationNumber;

                var logger = _loggerFactory.MakeLogger(Configuration.Name, simulationSession, simulationNumber);

                var fields = Configuration.Fields.Select(f => new FieldHistory(f)).ToList();
                var sosielModel = CreateSosielModel(Configuration, fields);

                Algorithm algorithm = new Algorithm(Configuration.SOSIELConfiguration);
                algorithm.Initialize();
                Climate = new Climate(Configuration.Parameters, Configuration.ClimateForecast);
                AgroHydrology = new AgroHydrology(logger, Configuration.Parameters, fields, Configuration.CropEvapTransList);
                
                RVAC = new RVAC(Configuration.Parameters);

                for (var seasonNumber = 1; seasonNumber <= Configuration.Parameters.NumOfSeasons; seasonNumber++)
                {
                    CheckStatus();
                    CurrentSeason = seasonNumber;

                    algorithm.Run(sosielModel);
                    ProcessSossielResult(seasonNumber, sosielModel, fields);
                    Climate.ProcessSeason(seasonNumber);
                    AgroHydrology.ProcessSeasonStart((decimal)sosielModel.WaterCurtailmentRate);

                    for (var dayNumber = 1; dayNumber < Configuration.DaysCount; dayNumber++)
                    {
                        CheckStatus();
                        CurrentDay = dayNumber;

                        var dailyClimate = Climate.GetDailyClimate(dayNumber);
                        AgroHydrology.ProcessDay(dayNumber, dailyClimate);
                    }

                    AgroHydrology.ProcessSeasonEnd();
                    RVAC.ProcessSeason(Configuration.MarketPrices.First(mp => mp.SeasonNumber == seasonNumber), 
                        0, 
                        0, 
                        0,
                        0, 
                        AgroHydrology.HarvestableAlfalfa, 
                        AgroHydrology.HarvestableBarley,
                        AgroHydrology.HarvestableWheat);

                    simulationResult.AddSeasonResult(new SeasonResult(seasonNumber, Climate, AgroHydrology, RVAC));
                }
                
                RaiseSimulationResultObtained(simulationResult);
            }
        }

        private void ProcessSosielResult(int seasonNumber, SosielModel sosielModel, List<FieldHistory> fieldHistories)
        {
            foreach (var fieldHistory in fieldHistories)
            {
                var chadField = sosielModel.Fields.First(cf => cf.Number == fieldHistory.Field.FieldNumber);

                fieldHistory.AddNewSeason(new FieldSeason(seasonNumber, ConvertStringToPlant(chadField.Plant)));
            }
        }

        private Plant ConvertStringToPlant(string plant)
        {
            if (plant.Equals("Alfalfa"))
                return Plant.Alfalfa;
            if (plant.Equals("Barley"))
                return Plant.Barley;
            if (plant.Equals("Wheat"))
                return Plant.Wheat;
            if (plant.Equals("Nothing"))
                return Plant.Nothing;

            throw new ArgumentOutOfRangeException(nameof(plant));
        }

        #endregion
    }
}