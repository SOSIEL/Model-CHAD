namespace CHAD.Model.AgroHydrologyModule
{
    public class Field
    {
        #region Constructors

        public Field(int fieldNumber, decimal fieldSize)
        {
            FieldNum = fieldNumber;
            FieldSize = fieldSize;
        }

        #endregion

        #region Public Interface

        public static bool operator ==(Field left, Field right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Field left, Field right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Field) obj);
        }

        public override int GetHashCode()
        {
            return FieldNum;
        }

        public int FieldNum { get; }
        public decimal FieldSize { get; }

        #endregion

        #region All other members

        private bool Equals(Field other)
        {
            return FieldNum == other.FieldNum;
        }

        #endregion
    }
}