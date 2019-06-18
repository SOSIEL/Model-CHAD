namespace CHAD.Model.Infrastructure
{
    public class DummyAgroHydrologyCalculationLogger : IAgroHydrologyCalculationLogger
    {
        public void AddRecord(int season, int day, string field, string property, object value)
        {
            
        }

        public void AddRecord(int season, int day, string property, object value)
        {
            
        }

        public void AddRecord(int season, string property, object value)
        {
           
        }

        public void Complete()
        {

        }
    }
}
