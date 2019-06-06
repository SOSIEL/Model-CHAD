namespace CHAD.Model.Infrastructure
{
    public class AgroHydrologyFieldRecord : AgroHydrologyRecord
    {
        public AgroHydrologyFieldRecord(int season, int day, string field, string recordName, string value) : base(
            season, day, recordName, value)
        {
            Field = field;
        }

        public string Field { get; }
    }
}