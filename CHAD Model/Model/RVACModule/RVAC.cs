namespace CHAD.Model.RVACModule
{
    public class RVAC
    {
        #region Fields

        private readonly Parameters _parameters;

        #endregion

        #region Constructors

        public RVAC(Parameters parameters)
        {
            _parameters = parameters;
        }

        #endregion

        #region Public Interface

        public void ProcessSeason(MarketPrice marketPrice, double numberOfAlfalfaAcres, double numberOfBarleyAcres,
            double numberOfCRPAcres, double numberOfWheatAcres, double harvestableAlfalfa, double harvestableBarley, double harvestableWheat)
        {
            ProfitAlfalfa = (marketPrice.MarketPriceAlfalfa - _parameters.CostAlfalfa) *
                            _parameters.MeanBushelsAlfalfaPerAcre *
                            numberOfAlfalfaAcres * harvestableAlfalfa;

            ProfitBarley = (marketPrice.MarketPriceBarley - _parameters.CostBarley) *
                           _parameters.MeanBushelsBarleyPerAcre *
                           numberOfBarleyAcres * harvestableBarley;

            ProfitWheat = (marketPrice.MarketPriceWheat - _parameters.CostWheat) * _parameters.MeanBushelsWheatPerAcre *
                          numberOfWheatAcres * harvestableWheat;

            ProfitCRP = marketPrice.SubsidyCRP * numberOfCRPAcres;

            ProfitTotal = ProfitAlfalfa + ProfitBarley + ProfitWheat + ProfitCRP;
        }

        public double ProfitAlfalfa { get; private set; }

        public double ProfitBarley { get; private set; }

        public double ProfitCRP { get; private set; }


        public double ProfitTotal { get; private set; }

        public double ProfitWheat { get; private set; }

        #endregion
    }
}