using System.Collections;
using System.Collections.Generic;
using CHAD.Model.RVACModule;

namespace CHAD.Model
{
    public class SimulationResult : IEnumerable<SeasonResult>
    {
        #region Fields

        private readonly List<SeasonResult> _seasonResults;

        #endregion

        #region Constructors

        public SimulationResult(string simulationSession, Configuration configuration, int simulationNumber)
        {
            SimulationSession = simulationSession;
            Configuration = configuration;
            SimulationNumber = simulationNumber;

            _seasonResults = new List<SeasonResult>();
        }

        #endregion

        #region Public Interface

        public void AddSeasonResult(SeasonResult seasonResult)
        {
            _seasonResults.Add(seasonResult);
        }

        public Configuration Configuration { get; }

        public IEnumerator<SeasonResult> GetEnumerator()
        {
            return _seasonResults.GetEnumerator();
        }

        public int SimulationNumber { get; }

        public string SimulationSession { get; }

        #endregion

        #region Interface Implementations

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}