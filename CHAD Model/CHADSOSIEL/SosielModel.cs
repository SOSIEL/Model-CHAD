using System.Collections.Generic;

namespace CHADSOSIEL
{
    public class SosielModel
    {
        #region Aquifire
        public double WaterInAquifire { get; set; }

        public double WaterCurtailmentRate { get; set; }

        public double WaterInAquiferMax { get; set; }

        public double SustainableLevelAquifer { get; set; }
        #endregion

        #region Profit
        public double MarketPriceAlfalfa { get; set; }

        public double MarketPriceBarley { get; set; }

        public double MarketPriceWheat { get; set; }

        public double CostAlfalfa { get; set; }

        public double CostBarley { get; set; }

        public double CostWheat { get; set; }

        public double MeanBushelsAlfalfaPerAcre { get; set; }

        public double MeanBushelsBarleyPerAcre { get; set; }

        public double MeanBushelsWheatPerAcre { get; set; }

        public double HarvestableAlfalfa { get; set; }

        public double HarvestableBarley { get; set; }

        public double HarvestableWheat { get; set; }

        public double BreakEvenPriceAlfalfa { get; set; }

        public double BreakEvenPriceBarley { get; set; }

        public double BreakEvenPriceWheat { get; set; }

        public double SubsidyCRP { get; set; }

        public double ProfitDoNothing { get; set; }
        #endregion

        
        public ICollection<ChadField> Fields { get; set; }
    }
}
