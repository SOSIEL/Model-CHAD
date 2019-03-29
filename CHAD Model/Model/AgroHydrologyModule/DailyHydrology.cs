namespace CHAD.Model.AgroHydrologyModule
{
    public class DailyHydrology
    {
        #region Constructors

        public DailyHydrology(int day, double waterInAquifer, double waterInSnowpack)
        {
            Day = day;
            WaterInAquifer = waterInAquifer;
            WaterInSnowpack = waterInSnowpack;
        }

        #endregion

        #region Public Interface

        public int Day { get; }

        public double WaterInAquifer { get; }

        public double WaterInSnowpack { get; }

        #endregion
    }
}