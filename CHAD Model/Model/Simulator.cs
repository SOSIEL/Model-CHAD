using System;

namespace Model
{
    public class Simulator
    {
        #region Fields

        private readonly ILogger _logger;

        private SimulatorStatus _status;

        #endregion

        #region Constructors

        public Simulator(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Public Interface

        public AgroHydrology AgroHydrology { get; private set; }

        public event Action StatusChanged;

        public Configuration Configuration { get; private set; }

        public SimulatorStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                StatusChanged?.Invoke();
            }
        }

        public void Start()
        {
            if (Status == SimulatorStatus.Run)
                return;

            Status = SimulatorStatus.Run;

            AgroHydrology = new AgroHydrology(_logger, Configuration.Parameters, Configuration.ClimateList,
                Configuration.Fields, Configuration.CropEvapTransList);

            Simulate();

            Stop();
        }

        public void Stop()
        {
            if (Status == SimulatorStatus.Stopped)
                return;

            Status = SimulatorStatus.Stopped;
        }

        public void Pause()
        {
            if (Status == SimulatorStatus.Stopped || Status == SimulatorStatus.OnPaused)
                return;

            Status = SimulatorStatus.OnPaused;
        }


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
            for (var seasonNumber = 1; seasonNumber <= Configuration.SeasonsCount; seasonNumber++)
            {
                for (var dayNumber = 1; dayNumber <= Configuration.DaysInSeasonCount; dayNumber++)
                {
                    AgroHydrology.ProcessDay(dayNumber);
                }
            }
        }

        #endregion
    }
}