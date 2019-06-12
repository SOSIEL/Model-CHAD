namespace CHAD.Model.Infrastructure
{
    public interface IAgroHydrologyCalculationLogger
    {
        void AddRecord(int season, int day, string field, string property, object value);

        void AddRecord(int season, int day, string property, object value);

        void AddRecord(int season, string property, object value);

        void Complete();
    }
}
