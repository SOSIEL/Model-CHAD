namespace CHAD.Model.RVACModule
{
    public class MarketPrice
    {
        #region Constructors

        public MarketPrice(int seasonNumber, decimal marketPriceAlfalfa, decimal marketPriceBarley, decimal marketPriceWheat,
            decimal subsidyCRP)
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

        public decimal MarketPriceAlfalfa { get; }

        public decimal MarketPriceBarley { get; }

        public decimal MarketPriceWheat { get; }

        public decimal SubsidyCRP { get; }

        #endregion
    }
}