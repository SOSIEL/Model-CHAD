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

        public void ProcessSeason(MarketPrice marketPrice, decimal numberOfAlfalfaAcres, decimal numberOfBarleyAcres,
            decimal numberOfCRPAcres, decimal numberOfWheatAcres
            , decimal harvestableAlfalfa, decimal harvestableBarley, decimal harvestableWheat)
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

        public decimal ProfitAlfalfa { get; private set; }

        public decimal ProfitBarley { get; private set; }

        public decimal ProfitCRP { get; private set; }


        public decimal ProfitTotal { get; private set; }

        public decimal ProfitWheat { get; private set; }

        #endregion
    }
}