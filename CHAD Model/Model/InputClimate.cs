namespace Model
{
    public class InputClimate
    {
        #region Public Interface

        public int t { get; set; }
        public double TempMean { get; set; }
        public double TempSD { get; set; }
        public double PrecipMean { get; set; }
        public double PrecipSD { get; set; }
        public decimal TempMeanRandom { get; set; }
        public decimal PrecipMeanRandom { get; set; }

        #endregion
    }
}