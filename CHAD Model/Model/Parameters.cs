namespace CHAD.Model
{
    public class Parameters
    {
        public Parameters()
        {
            NumOfSimulations = 1;
            NumOfSeasons = 1;

            Beta = 1;
            LeakAquiferFrac = 0;
            PercFromFieldFrac = 1;
            ProfitDoNothing = 0;
            WaterInAquifer = 0;
            WaterInAquiferMax = 10;
            WaterCurtailmentRate = 12.6m;
            WaterUsageMax = WaterCurtailmentBase * (1 - WaterCurtailmentRate / 100);
            WaterStoreCap = 1;
        }

        #region Public Members

        public int NumOfSimulations { get; set; }

        public int NumOfSeasons { get; set; }

        public decimal Beta { get; set; }

        public decimal MeanBushelsAlfalfaPerAcre { get; set; }

        public decimal MeanBushelsBarleyPerAcre { get; set; }

        public decimal MeanBushelsWheatPerAcre { get; set; }

        public decimal CostAlfalfa { get; set; }

        public decimal CostBarley { get; set; }

        public decimal CostWheat { get; set; }

        public decimal WaterCurtailmentBase { get; set; }

        public decimal WaterCurtailmentRate { get; set; }

        public decimal WaterUsageMax { get; set; }

        public decimal LeakAquiferFrac { get; set; }

        public decimal PercFromFieldFrac { get; set; }

        public decimal ProfitDoNothing { get; set; }

        public decimal SustainableLevelAquifer { get; set; }

        public decimal WaterInAquifer { get; set; }

        public decimal WaterInAquiferMax { get; set; }

        public decimal WaterStoreCap { get; set; }

        #endregion
    }
}