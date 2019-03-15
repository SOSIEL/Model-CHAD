namespace Model
{
    public class Climate
    {
        #region Public Interface

        public int Day { get; set; }
        public double TempMean { get; set; }
        public double TempSD { get; set; }
        public double PrecipMean { get; set; }
        public double PrecipSD { get; set; }
        public decimal TempMeanRandom { get; set; }
        public decimal PrecipMeanRandom { get; set; }

        #endregion
    }
}