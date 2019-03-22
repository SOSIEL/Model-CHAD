namespace CHAD.Model.Infrastructure
{
    public interface ILoggerFactory
    {
        ILogger MakeLogger(string configurationName, string simulationSession, int simulationNumber);
    }
}
