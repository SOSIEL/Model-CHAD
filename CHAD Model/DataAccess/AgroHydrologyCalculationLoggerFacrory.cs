using CHAD.Model.Infrastructure;

namespace CHAD.DataAccess
{
    public class AgroHydrologyCalculationLoggerFactory : IAgroHydrologyCalculationLoggerFactory
    {
        private readonly IStorageService _storageService;
        private readonly SaveFrequency _saveFrequency;

        public AgroHydrologyCalculationLoggerFactory(IStorageService storageService, SaveFrequency saveFrequency)
        {
            _storageService = storageService;
            _saveFrequency = saveFrequency;
        }

        public IAgroHydrologyCalculationLogger MakeLogger(SimulationInfo simulationInfo)
        {
            return new AgroHydrologyCalculationLogger(_storageService, _saveFrequency, simulationInfo);
        }
    }
}