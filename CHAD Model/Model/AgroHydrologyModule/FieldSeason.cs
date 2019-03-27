namespace CHAD.Model.AgroHydrologyModule
{
    public class FieldSeason
    {
        #region Constructors

        public FieldSeason(int seasonNumber)
        {
            SeasonNumber = seasonNumber;
        }

        #endregion

        #region Public Interface

        public double Harvestable { get; set; }

        public Plant Plant { get; set; }

        public int SeasonNumber { get; }

        #endregion
    }
}