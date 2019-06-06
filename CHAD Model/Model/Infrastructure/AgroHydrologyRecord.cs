namespace CHAD.Model.Infrastructure
{
    public class AgroHydrologyRecord
    {
        #region Constructors

        public AgroHydrologyRecord(int season, int day, string recordName, string value)
        {
            Season = season;
            Day = day;
            RecordName = recordName;
            Value = value;
        }

        #endregion

        #region Public Interface

        public int Day { get; }

       

        public string RecordName { get; }

        public int Season { get; }

        public string Value { get; }

        #endregion
    }
}