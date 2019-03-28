namespace CHAD.Model.AgroHydrologyModule
{
    public class DailyHydrology
    {
        #region Constructors

        public DailyHydrology(int day, decimal waterInAquifer, decimal waterInSnowpack)
        {
            Day = day;
            WaterInAquifer = waterInAquifer;
            WaterInSnowpack = waterInSnowpack;
        }

        #endregion

        #region Public Interface

        public int Day { get; }

        public decimal WaterInAquifer { get; }

        public decimal WaterInSnowpack { get; }

        #endregion
    }
}