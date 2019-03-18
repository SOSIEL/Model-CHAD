using Model;

namespace DataAccess
{
    public class FileLoggerFactory : ILoggerFactory
    {
        #region Public Interface

        public ILogger MakeLogger(string configurationName, string simulationSession, int simulationNumber)
        {
            var folderPath =
                FileStorageService.MakeSimulationPath(configurationName, simulationSession, simulationNumber);

            return new FileLogger(folderPath);
        }

        #endregion
    }
}