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

        public void SetConfiguration(Configuration configuration)
        {
            if (Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Unable to change configuration while simulator is working");

            Configuration = configuration;
        }

        public event Action<SimulationResult> SimulationResultObtained;

        public void Start()
        {
            if (Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Simulator is already running");

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

        private SosielModel CreateSosielModel(double waterInAquifer, List<Field> fields)
        {
            var model = new SosielModel
            {
                WaterInAquifire = waterInAquifer
            };

            foreach (var field in fields)
            {
                model.Fields.Add(new ChadField
                {
                    FieldHistoryCrop = field.GetCropNumberSeasons(),
                    FieldHistoryNonCrop = field.GetNonCropNumberSeasons(),
                    //ProfitCRP = (double)Configuration.Parameters.ProfitCRP
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

                Climate = new Climate(Configuration.Parameters, Configuration.ClimateForecast);
                AgroHydrology = new AgroHydrology(logger, Configuration.Parameters,
                    Configuration.Fields, Configuration.CropEvapTransList);
                var sosielModel = CreateSosielModel((double)Configuration.Parameters.WaterInAquifer, Configuration.Fields);
                RVAC = new RVAC(Configuration.Parameters);

                for (var seasonNumber = 1; seasonNumber <= Configuration.Parameters.NumOfSeasons; seasonNumber++)
                {
                    CheckStatus();
                    CurrentSeason = seasonNumber;

                    Climate.ProcessSeason(seasonNumber);
                    AgroHydrology.ProcessSeasonStart();

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

        #endregion
    }
}