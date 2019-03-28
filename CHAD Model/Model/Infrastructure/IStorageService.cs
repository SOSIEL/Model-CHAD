using System.Collections.Generic;

namespace CHAD.Model.Infrastructure
{
    public interface IStorageService
    {
        #region Public Interface

        IEnumerable<Configuration> GetConfigurations();

        Configuration GetConfiguration(Configuration configuration);

        void SaveConfiguration(Configuration configuration, bool rewrite);

        void SaveSimulationResult(SimulationResult simulationResult);

        #endregion
    }
}