using System;

namespace CHAD.Model
{
    public class Parameters : ICloneable
    {
        #region Constructors

        public Parameters()
        {
            NumOfSimulations = 1;
            NumOfSeasons = 1;
            UseDemographicProcesses = false;

            MeanBushelsAlfalfaPerAcre = 0;
            MeanBushelsBarleyPerAcre = 0;
            MeanBushelsWheatPerAcre = 0;

            CostAlfalfa = 0;
            CostBarley = 0;
            CostWheat = 0;
            ProfitCRP = 0;
            WaterCurtailmentBase = 6;
            WaterCurtailmentRate = 12.6m;

            Beta = 1;
            LeakAquiferFrac = 0;
            PercFromFieldFrac = 1;
            SustainableLevelAquifer = 5;
            WaterInAquifer = 5;
            WaterInAquiferMax = 10;
            WaterStoreCap = 1;
        }

        #endregion

        #region Public Interface

        public Parameters Clone()
        {
            return ((ICloneable) this).Clone() as Parameters;
        }

        public int NumOfSimulations { get; set; }

        public int NumOfSeasons { get; set; }

        public bool UseDemographicProcesses { get; set; }


        public decimal MeanBushelsAlfalfaPerAcre { get; set; }

        public decimal MeanBushelsBarleyPerAcre { get; set; }

        public decimal MeanBushelsWheatPerAcre { get; set; }


        public decimal CostAlfalfa { get; set; }

        public decimal CostBarley { get; set; }

        public decimal CostWheat { get; set; }

        public decimal ProfitCRP { get; set; }

        public decimal WaterCurtailmentBase { get; set; }

        public decimal WaterCurtailmentRate { get; set; }


        public decimal Beta { get; set; }

        public decimal LeakAquiferFrac { get; set; }

        public decimal PercFromFieldFrac { get; set; }

        public decimal SustainableLevelAquifer { get; set; }

        public decimal WaterInAquifer { get; set; }

        public decimal WaterInAquiferMax { get; set; }

        public decimal WaterStoreCap { get; set; }

        #endregion

        #region Interface Implementations

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}