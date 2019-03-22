using System;
using System.Threading;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.Infrastructure;

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

        public event Action<SimulationResult> SimulationResultObtained;

        public Climate Climate { get; private set; }

        public AgroHydrology AgroHydrology { get; private set; }

        public event Action StatusChanged;

        public Configuration Configuration { get; private set; }

        public SimulatorStatus Status
        {
            get => _status;
            private set
            {
                _status = value;
                StatusChanged?.Invoke();
            }
        }

        public void Start()
        {
            if (Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Simulator is already running");

            Status = SimulatorStatus.Run;

            Simulate();

            Stop();
        }

        public void Stop()
        {
            if (Status == SimulatorStatus.Stopped)
                throw new InvalidOperationException("Simulator is already stopped");

            CurrentSimulation = 0;
            CurrentSeason = 0;
            CurrentDay = 0;

            Status = SimulatorStatus.Stopped;
        }

        public void Pause()
        {
            if (Status == SimulatorStatus.OnPaused)
                throw new InvalidOperationException("Simulator is already on pause");

            Status = SimulatorStatus.OnPaused;
        }

        public void Continue()
        {
            if (Status != SimulatorStatus.OnPaused)
                throw new InvalidOperationException("Simulator is not on pause");

            Status = SimulatorStatus.Run;
        }

        public int CurrentSimulation { get; private set; }

        public int CurrentSeason { get; private set; }

        public int CurrentDay { get; private set; }

        public void SetConfiguration(Configuration configuration)
        {
            if (Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Unable to change configuration while simulator is working");

            Configuration = configuration;
        }

        #endregion

        #region All other members

        private void Simulate()
        {
            var simulationSession = MakeSimulationSession();

            for (var simulationNumber = 1;
                simulationNumber <= Configuration.Parameters.NumOfSimulations;
                simulationNumber++)
            {
                CheckStatus();
                CurrentSimulation = simulationNumber;

                var logger = _loggerFactory.MakeLogger(Configuration.Name, simulationSession, simulationNumber);

                for (var seasonNumber = 1; seasonNumber <= Configuration.Parameters.NumOfSeasons; seasonNumber++)
                {
                    CheckStatus();
                    CurrentSeason = seasonNumber;

                    Climate = new Climate(Configuration.ClimateForecast);
                    AgroHydrology = new AgroHydrology(logger, Configuration.Parameters,
                        Configuration.Fields, Configuration.CropEvapTransList);

                    for (var dayNumber = 1; dayNumber < Configuration.DaysCount; dayNumber++)
                    {
                        CheckStatus();
                        CurrentDay = dayNumber;

                        var dailyClimate = Climate.GetDailyClimate(dayNumber);
                        AgroHydrology.ProcessDay(dayNumber, dailyClimate);
                    }
                }

                var simulationResults = new SimulationResult(simulationSession, Configuration, simulationNumber,
                    Climate, AgroHydrology);
                RaiseSimulationResultObtained(simulationResults);
            }
        }

        private string MakeSimulationSession()
        {
            return DateTimeOffset.Now.ToString("yyyy.MM.dd -- HH-mm-ss");
        }

        private void CheckStatus()
        {
            while (Status == SimulatorStatus.OnPaused)
                Thread.Sleep(250);

            if (Status == SimulatorStatus.Stopped)
                Thread.CurrentThread.Abort();
        }

        private void RaiseSimulationResultObtained(SimulationResult simulationResult)
        {
            SimulationResultObtained?.Invoke(simulationResult);
        }

        #endregion
    }
}