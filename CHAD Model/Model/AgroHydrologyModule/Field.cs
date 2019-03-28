namespace CHAD.Model.AgroHydrologyModule
{
    public class Field
    {
        #region Constructors

        public Field(int fieldNumber, decimal fieldSize)
        {
            FieldNumber = fieldNumber;
            FieldSize = fieldSize;
        }

        #endregion

        #region Public Interface

        public int FieldNumber { get; }
        public decimal FieldSize { get; }

        #endregion
    }
}