namespace CHAD.Model.Infrastructure
{
    public interface ILoggerFactory
    {
        ILogger MakeLogger(Configuration configuration, string simulationSession, int simulationNumber);
    }
}
