namespace CHAD.Model.RVACModule
{
    public class MarketPrice
    {
        #region Constructors

        public MarketPrice(int seasonNumber, double marketPriceAlfalfa, double marketPriceBarley, double marketPriceWheat,
            double subsidyCRP)
        {
            SeasonNumber = seasonNumber;
            MarketPriceAlfalfa = marketPriceAlfalfa;
            MarketPriceBarley = marketPriceBarley;
            MarketPriceWheat = marketPriceWheat;
            SubsidyCRP = subsidyCRP;
        }

        #endregion

        #region Public Interface

        public int SeasonNumber { get; }

        public double MarketPriceAlfalfa { get; }

        public double MarketPriceBarley { get; }

        public double MarketPriceWheat { get; }

        public double SubsidyCRP { get; }

        #endregion
    }
}