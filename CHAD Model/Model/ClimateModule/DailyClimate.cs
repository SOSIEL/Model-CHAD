namespace CHAD.Model.ClimateModule
{
    public class DailyClimate
    {
        #region Constructors

        public DailyClimate(int day, double temperature, double precipitation)
        {
            Day = day;
            Temperature = temperature;
            Precipitation = precipitation;
        }

        #endregion

        #region Public Interface

        public int Day { get; }
        public double Temperature { get; }
        public double Precipitation { get; }

        #endregion

        
    }
}