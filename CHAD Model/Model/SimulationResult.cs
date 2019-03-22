using System;
using Model.Modules;

namespace Model
{
    public class SimulationResult
    {
        #region Constructors

        public SimulationResult(string simulationSession, Configuration configuration, int simulationNumber,
            AgroHydrology agroHydrology)
        {
            SimulationSession = simulationSession;
            Configuration = configuration;
            SimulationNumber = simulationNumber;
            AgroHydrology = agroHydrology;
        }

        #endregion

        #region Public Interface
        public string SimulationSession { get; }
        public Configuration Configuration { get; }
        public int SimulationNumber { get; }
        public AgroHydrology AgroHydrology { get; }

        #endregion
    }
}