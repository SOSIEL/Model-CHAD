namespace CHAD.Model.ClimateModule
{
    public class ClimateForecast
    {
        #region Public Interface

        public int Day { get; set; }
        public double TempMean { get; set; }
        public double TempSD { get; set; }
        public double PrecipMean { get; set; }
        public double PrecipSD { get; set; }

        #endregion
    }
}