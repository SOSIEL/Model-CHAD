using Model.AgroHydrologyModule;
using Model.ClimateModule;

namespace Model
{
    public class SimulationResult
    {
        #region Constructors

        public SimulationResult(string simulationSession, Configuration configuration, int simulationNumber,
            Climate climate, AgroHydrology agroHydrology)
        {
            SimulationSession = simulationSession;
            Configuration = configuration;
            SimulationNumber = simulationNumber;
            Climate = climate;
            AgroHydrology = agroHydrology;
        }

        #endregion

        #region Public Interface

        public string SimulationSession { get; }
        public Configuration Configuration { get; }
        public int SimulationNumber { get; }
        public Climate Climate { get; }
        public AgroHydrology AgroHydrology { get; }

        #endregion
    }
}