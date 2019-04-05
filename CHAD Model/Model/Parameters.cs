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
            NumOfDays = 153;
            UseDemographicProcesses = false;

            MeanBushelsAlfalfaPerAcre = 5;
            MeanBushelsBarleyPerAcre = 135;
            MeanBushelsWheatPerAcre = 130;

            ClimateChangePrecipMean = 1;
            ClimateChangePrecipSD = 1;
            ClimateChangeTempMean = 1;
            ClimateChangeTempSD = 1;

            CostAlfalfa = 81.09;
            CostBarley = 2.96;
            CostWheat = 3.23;
            ProfitCRP = 0;
            WaterCurtailmentBase = 8400000;
            WaterCurtailmentRate = 12.6;

            Beta = 2;
            FieldDepth = 72;
            LeakAquiferFrac = 0;
            PercFromFieldFrac = 0.25;
            SustainableLevelAquifer = 1000000;
            WaterInAquifer = 1500000;
            WaterInAquiferMax = 10000000;
            MeltingPoint = 32;
            WaterInSnowpack = 0;
        }

        #endregion

        #region Public Interface

        public Parameters Clone()
        {
            return ((ICloneable) this).Clone() as Parameters;
        }

        public string SosielConfiguration { get; set; }

        public int NumOfSimulations { get; set; }

        public int NumOfSeasons { get; set; }

        public int NumOfDays { get; set; }

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

        public double FieldDepth { get; set; }

        public double LeakAquiferFrac { get; set; }

        public double PercFromFieldFrac { get; set; }

        public double SustainableLevelAquifer { get; set; }

        public double WaterInAquifer { get; set; }

        public double WaterInAquiferMax { get; set; }

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