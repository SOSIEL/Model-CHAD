namespace CHAD.Model.Infrastructure
{
    public interface IAgroHydrologyCalculationLoggerFactory
    {
        IAgroHydrologyCalculationLogger MakeLogger(Configuration configuration, SimulationInfo simulationInfo);
    }
}
