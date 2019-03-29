namespace CHAD.Model.AgroHydrologyModule
{
    public class Field
    {
        #region Constructors

        public Field(int fieldNumber, double fieldSize)
        {
            FieldNumber = fieldNumber;
            FieldSize = fieldSize;
        }

        #endregion

        #region Public Interface

        public int FieldNumber { get; }
        public double FieldSize { get; }

        #endregion
    }
}