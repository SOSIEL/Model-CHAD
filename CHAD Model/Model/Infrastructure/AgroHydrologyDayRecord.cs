namespace CHAD.Model.Infrastructure
{
    public class AgroHydrologyDayRecord : AgroHydrologyRecord
    {
        #region Constructors

        public AgroHydrologyDayRecord(int season, int day, string recordName, object value) 
            : base(season, recordName, value)
        {
            Day = day;
        }

        #endregion

        #region Public Interface

        public int Day { get; }

        #endregion
    }
}