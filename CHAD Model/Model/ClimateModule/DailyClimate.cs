namespace Model.ClimateModule
{
    public class DailyClimate
    {
        #region Constructors

        public DailyClimate(int day, decimal temperature, decimal precipitation)
        {
            Day = day;
            Temperature = temperature;
            Precipitation = precipitation;
        }

        #endregion

        #region Public Interface

        public int Day { get; }
        public decimal Temperature { get; }
        public decimal Precipitation { get; }

        #endregion

        
    }
}