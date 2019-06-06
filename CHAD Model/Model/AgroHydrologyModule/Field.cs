namespace CHAD.Model.AgroHydrologyModule
{
    public class Field
    {
        #region Constructors

        public Field(int fieldNumber, double fieldSize, double initialWaterVolume)
        {
            FieldNumber = fieldNumber;
            FieldSize = fieldSize;
            InitialWaterVolume = initialWaterVolume;
        }

        #endregion

        #region Public Interface

        public int FieldNumber { get; }

        public double FieldSize { get; }

        public double InitialWaterVolume { get; }

        #endregion
    }
}