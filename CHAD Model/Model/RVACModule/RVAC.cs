using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.SimulationResults;

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

        public void ProcessSeason(MarketPrice marketPrice, SOSIELResult sosielResult, AgroHydrology agroHydrology)
        {
            ProfitAlfalfa = (marketPrice.MarketPriceAlfalfa - _parameters.CostAlfalfa) *
                            _parameters.MeanAlfalfa *
                            sosielResult.NumOfAlfalfaAcres * agroHydrology.HarvestableAlfalfa;

            ProfitBarley = (marketPrice.MarketPriceBarley - _parameters.CostBarley) *
                           _parameters.MeanBarley *
                           sosielResult.NumOfBarleyAcres * agroHydrology.HarvestableBarley;

            ProfitWheat = (marketPrice.MarketPriceWheat - _parameters.CostWheat) * _parameters.MeanWheat *
                          sosielResult.NumOfWheatAcres * agroHydrology.HarvestableWheat;

            ProfitCRP = marketPrice.SubsidyCRP * sosielResult.NumOfCRPAcres;

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