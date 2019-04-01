using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.Infrastructure;
using CHAD.Model.RVACModule;
using CHAD.Model.SimulationResults;
using CHADSOSIEL;

namespace CHAD.Model
{
    public class Simulator
    {
        #region Fields

        private readonly ILoggerFactory _loggerFactory;

        private Configuration _configuration;

        private SimulatorStatus _status;

        #endregion

        #region Constructors

        public Simulator(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        #endregion

        #region Public Interface

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

        public event Action<SimulationResult> SimulationResultObtained;

        public void Start(Configuration configuration)
        {
            if (Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Simulator is already running");

            _configuration = configuration;

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

        private void CheckStatus()
        {
            while (Status == SimulatorStatus.OnPaused)
                Thread.Sleep(250);

            if (Status == SimulatorStatus.Stopped)
                Thread.CurrentThread.Abort();
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

        private SosielModel CreateSosielModel(Configuration configuration, List<FieldHistory> fieldHistories)
        {
            var model = new SosielModel
            {
                WaterInAquifire = configuration.Parameters.WaterInAquifer,
                WaterInAquiferMax = configuration.Parameters.WaterInAquiferMax,
                SustainableLevelAquifer = configuration.Parameters.SustainableLevelAquifer
            };

            model.Fields = new List<ChadField>();

            foreach (var fieldHistory in fieldHistories)
                model.Fields.Add(new ChadField
                {
                    Number = fieldHistory.Field.FieldNumber,
                    FieldHistoryCrop = fieldHistory.GetCropNumberSeasons(),
                    FieldHistoryNonCrop = fieldHistory.GetNonCropNumberSeasons()
                });

            return model;
        }

        private void FillSosielModel(SosielModel sosielModel, AgroHydrology agroHydrology, MarketPrice marketPrice, RVAC rvac)
        {
            sosielModel.ProfitAlfalfa = rvac.ProfitAlfalfa;
            sosielModel.ProfitBarley = rvac.ProfitBarley;
            sosielModel.ProfitWheat = rvac.ProfitWheat;
            sosielModel.ProfitCRP = rvac.ProfitCRP;
            sosielModel.ProfitTotal = rvac.ProfitTotal;
            sosielModel.ProfitDoNothing = 0;
            sosielModel.CostAlfalfa = _configuration.Parameters.CostAlfalfa;
            sosielModel.CostBarley = _configuration.Parameters.CostBarley;
            sosielModel.CostWheat = _configuration.Parameters.CostWheat;;
            sosielModel.MarketPriceAlfalfa = marketPrice.MarketPriceAlfalfa;
            sosielModel.MarketPriceBarley = marketPrice.MarketPriceBarley;
            sosielModel.MarketPriceWheat = marketPrice.MarketPriceWheat;
            sosielModel.SubsidyCRP = marketPrice.SubsidyCRP;
            sosielModel.HarvestableAlfalfa = agroHydrology.HarvestableAlfalfa;
            sosielModel.HarvestableBarley = agroHydrology.HarvestableBarley;
            sosielModel.HarvestableWheat = agroHydrology.HarvestableWheat;
            sosielModel.WaterInAquiferMax = _configuration.Parameters.WaterInAquiferMax;
            sosielModel.SustainableLevelAquifer = _configuration.Parameters.SustainableLevelAquifer;
            sosielModel.WaterInAquifire = agroHydrology.WaterInAquifer;
        }

        private string MakeSimulationSession()
        {
            return DateTimeOffset.Now.ToString("yyyy.MM.dd -- HH-mm-ss");
        }

        private SOSIELResult ProcessSosielResult(int seasonNumber, SosielModel sosielModel, List<FieldHistory> fieldHistories)
        {
            foreach (var fieldHistory in fieldHistories)
            {
                var chadField = sosielModel.Fields.First(cf => cf.Number == fieldHistory.Field.FieldNumber);

                fieldHistory.AddNewSeason(new FieldSeason(seasonNumber, ConvertStringToPlant(chadField.Plant)));
            }

            double numOfAlfalfaAcres =
                fieldHistories.Where(fh => fh.Plant == Plant.Alfalfa).Sum(fh => fh.Field.FieldSize);

            double numOfBarleyAcres =
                fieldHistories.Where(fh => fh.Plant == Plant.Barley).Sum(fh => fh.Field.FieldSize);

            double numOfCRPAcres =
                fieldHistories.Where(fh => fh.Plant == Plant.Nothing).Sum(fh => fh.Field.FieldSize);

            double numOfWheatAcres =
                fieldHistories.Where(fh => fh.Plant == Plant.Wheat).Sum(fh => fh.Field.FieldSize);

            return new SOSIELResult(numOfAlfalfaAcres,numOfBarleyAcres,numOfCRPAcres,numOfWheatAcres);
        }

        private void RaiseSimulationResultObtained(SimulationResult simulationResult)
        {
            SimulationResultObtained?.Invoke(simulationResult);
        }

        private void Simulate()
        {
            var simulationSession = MakeSimulationSession();

            for (var simulationNumber = 1;
                simulationNumber <= _configuration.Parameters.NumOfSimulations;
                simulationNumber++)
            {
                var simulationResult = new SimulationResult(simulationSession, _configuration, simulationNumber);

                CheckStatus();
                CurrentSimulation = simulationNumber;

                var logger = _loggerFactory.MakeLogger(_configuration.Name, simulationSession, simulationNumber);

                var fieldHistories = _configuration.Fields.Select(f => new FieldHistory(f)).ToList();
                var sosielModel = CreateSosielModel(_configuration, fieldHistories);

                var climate = new Climate(_configuration.Parameters, _configuration.ClimateForecast);
                var agroHydrology = new AgroHydrology(logger, _configuration.Parameters, _configuration.Fields,
                    _configuration.CropEvapTransList);
                var rvac = new RVAC(_configuration.Parameters);
                var algorithm = new Algorithm(_configuration.SOSIELConfiguration);
                algorithm.Initialize(sosielModel);

                for (var seasonNumber = 1; seasonNumber <= _configuration.Parameters.NumOfSeasons; seasonNumber++)
                {
                    CheckStatus();
                    CurrentSeason = seasonNumber;

                    var marketPrice = _configuration.MarketPrices.First(mp => mp.SeasonNumber == seasonNumber);

                    FillSosielModel(sosielModel, agroHydrology, marketPrice, rvac);
                    algorithm.Run(sosielModel);
                    var sosielResult = ProcessSosielResult(seasonNumber, sosielModel, fieldHistories);
                    climate.ProcessSeason(seasonNumber);
                    agroHydrology.ProcessSeasonStart(fieldHistories, sosielModel.WaterCurtailmentRate);

                    for (var dayNumber = 1; dayNumber < _configuration.DaysCount; dayNumber++)
                    {
                        CheckStatus();
                        CurrentDay = dayNumber;

                        var dailyClimate = climate.GetDailyClimate(dayNumber);
                        agroHydrology.ProcessDay(dayNumber, dailyClimate, fieldHistories);
                    }

                    agroHydrology.ProcessSeasonEnd(fieldHistories);
                    rvac.ProcessSeason(marketPrice, sosielResult, agroHydrology);

                    simulationResult.AddSeasonResult(new SeasonResult(seasonNumber, sosielModel.WaterCurtailmentRate,
                        climate, agroHydrology, rvac));
                }

                RaiseSimulationResultObtained(simulationResult);
            }
        }

        #endregion
    }
}