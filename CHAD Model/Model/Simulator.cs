using System;

namespace Model
{
    public class Simulator
    {
        #region Fields

        private SimulatorStatus _status;

        #endregion

        #region Public Interface

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

            Simulate();
        }

        public void Stop()
        {
            if(Status == SimulatorStatus.Stopped)
                return;
            
            Status = SimulatorStatus.Stopped;
        }

        public void Pause()
        {
            if(Status == SimulatorStatus.Stopped || Status == SimulatorStatus.OnPaused)
                return;

            Status = SimulatorStatus.OnPaused;
        }


        public void SetConfiguration(Configuration configuration)
        {
            if(Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Unable to change configuration while simulator is working");

            Configuration = configuration;
        }

        #endregion

        private void Simulate()
        {
            for (int seasonNumber = 1; seasonNumber <= Configuration.SeasonsCount; seasonNumber++)
            {
                for (int dayNumber = 1; dayNumber <= Configuration.DaysInSeasonCount; dayNumber++)
                {
                    
                }
            }
        }
    }
}