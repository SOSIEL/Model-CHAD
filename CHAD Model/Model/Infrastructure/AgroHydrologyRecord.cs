namespace CHAD.Model.Infrastructure
{
    public class AgroHydrologyRecord
    {
        #region Constructors

        public AgroHydrologyRecord(int season, string recordName, object value)
        {
            Season = season;
            RecordName = recordName;
            Value = value;
        }

        #endregion

        #region Public Interface

        public string RecordName { get; }

        public int Season { get; }

        public object Value { get; }

        #endregion
    }
}