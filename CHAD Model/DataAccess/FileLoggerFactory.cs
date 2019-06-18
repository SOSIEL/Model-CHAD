using CHAD.Model;
using CHAD.Model.Infrastructure;

namespace CHAD.DataAccess
{
    public class FileLoggerFactory : ILoggerFactory
    {
        #region Public Interface

        public ILogger MakeLogger(Configuration configuration, string simulationSession, int simulationNumber)
        {
            if (configuration.Parameters.GenerateDetailedOutput)
            {
                var folderPath =
                    FileStorageService.MakeSimulationPath(configuration.Name, simulationSession, simulationNumber);

                return new FileLogger(folderPath);
            }

            return new DummyLogger();
        }

        #endregion
    }
}