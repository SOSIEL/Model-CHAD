namespace CHAD.Model.Infrastructure
{
    public interface IAgroHydrologyCalculationLoggerFactory
    {
        IAgroHydrologyCalculationLogger MakeLogger(SimulationInfo simulationInfo);
    }
}
