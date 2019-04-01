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

            ClimateChangePrecipMean = 1;
            ClimateChangePrecipSD = 1;
            ClimateChangeTempMean = 1;
            ClimateChangeTempSD = 1;

            CostAlfalfa = 0;
            CostBarley = 0;
            CostWheat = 0;
            ProfitCRP = 0;
            WaterCurtailmentBase = 6;
            WaterCurtailmentRate = 12.6;

            Beta = 1;
            LeakAquiferFrac = 0;
            PercFromFieldFrac = 1;
            SustainableLevelAquifer = 5;
            WaterInAquifer = 5;
            WaterInAquiferMax = 10;
            WaterStoreCap = 1;
            MeltingPoint = 32;
            WaterInSnowpack = 10;
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


        public double MeanBushelsAlfalfaPerAcre { get; set; }

        public double MeanBushelsBarleyPerAcre { get; set; }

        public double MeanBushelsWheatPerAcre { get; set; }


        public double ClimateChangePrecipMean { get; set; }

        public double ClimateChangePrecipSD { get; set; }

        public double ClimateChangeTempMean { get; set; }

        public double ClimateChangeTempSD { get; set; }


        public double CostAlfalfa { get; set; }

        public double CostBarley { get; set; }

        public double CostWheat { get; set; }

        public double ProfitCRP { get; set; }

        public double WaterCurtailmentBase { get; set; }

        public double WaterCurtailmentRate { get; set; }


        public double Beta { get; set; }

        public double LeakAquiferFrac { get; set; }

        public double PercFromFieldFrac { get; set; }

        public double SustainableLevelAquifer { get; set; }

        public double WaterInAquifer { get; set; }

        public double WaterInAquiferMax { get; set; }

        public double WaterStoreCap { get; set; }

        public double MeltingPoint { get; set; }

        public double WaterInSnowpack { get; set; }

        #endregion

        #region Interface Implementations

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}