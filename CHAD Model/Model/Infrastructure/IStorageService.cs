using System.Collections.Generic;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;

namespace CHAD.Model.Infrastructure
{
    public interface IStorageService
    {
        #region Public Interface

        IEnumerable<Configuration> GetConfigurations();

        void SaveLogs(string path, SimpleLogger logger);

        void SaveClimate(string path, Climate climate);

        void SaveHydrology(string path, List<Hydrology> hydrology, IEnumerable<Field> inputFieldSize);

        Configuration GetConfiguration(Configuration configuration);

        void SaveConfiguration(Configuration configuration, bool rewrite);

        void SaveSimulationResult(SimulationResult simulationResult,
            SimulationResultPart simulationResultPart = SimulationResultPart.Parameters | SimulationResultPart.Climate |
                                                        SimulationResultPart.Hydrology);

        #endregion
    }
}