namespace CHAD.Model.Infrastructure
{
    public interface IAgroHydrologyCalculationLogger
    {
        void AddFieldRecord(int season, int day, string field, string property, string value);

        void AddRecord(int season, int day, string property, string value);

        void Complete();
    }
}
