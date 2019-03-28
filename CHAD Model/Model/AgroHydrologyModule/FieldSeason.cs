namespace CHAD.Model.AgroHydrologyModule
{
    public class FieldSeason
    {
        #region Constructors

        public FieldSeason(int seasonNumber, Plant plant)
        {
            SeasonNumber = seasonNumber;
            Plant = plant;
        }

        #endregion

        #region Public Interface

        public double Harvestable { get; set; }

        public Plant Plant { get; }

        public int SeasonNumber { get; }

        #endregion
    }
}