namespace CHAD.Model.Infrastructure
{
    public class AgroHydrologyFieldRecord : AgroHydrologyDayRecord
    {
        #region Constructors

        public AgroHydrologyFieldRecord(int season, int day, string field, string recordName, object value) : base(
            season, day, recordName, value)
        {
            Field = field;
        }

        #endregion

        #region Public Interface

        public string Field { get; }

        #endregion
    }
}