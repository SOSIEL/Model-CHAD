namespace CHAD.Model.AgroHydrologyModule
{
    public class PlantInField
    {
        #region Constructors

        public PlantInField(Field field, Plant plant)
        {
            Field = field;
            Plant = plant;
        }

        #endregion

        #region Public Interface

        public Field Field { get; }

        public Plant Plant { get; }

        #endregion
    }
}