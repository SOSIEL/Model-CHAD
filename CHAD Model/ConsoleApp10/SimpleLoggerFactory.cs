using CHAD.Model.Infrastructure;

namespace CHAD.ConsoleApp10
{
    public class SimpleLoggerFactory : ILoggerFactory
    {
        #region Constructors

        public SimpleLoggerFactory()
        {
            Logger = new SimpleLogger();
        }

        #endregion

        #region Public Interface

        public SimpleLogger Logger { get; }

        public ILogger MakeLogger(string configurationName, string simulationSession, int simulationNumber)
        {
            return Logger;
        }

        #endregion
    }
}