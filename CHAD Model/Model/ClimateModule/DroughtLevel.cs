namespace CHAD.Model.ClimateModule
{
    public struct DroughtLevel
    {
        public DroughtLevel(int seasonNumber, double value)
        {
            SeasonNumber = seasonNumber;
            Value = value;
        }

        public int SeasonNumber { get; }

        public double Value { get; }
    }
}