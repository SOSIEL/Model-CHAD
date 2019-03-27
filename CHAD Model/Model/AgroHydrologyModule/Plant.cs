namespace CHAD.Model.AgroHydrologyModule
{
    public class Plant
    {
        #region Constructors

        public Plant(string name)
        {
            Name = name;
        }

        #endregion

        #region Public Interface

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Plant) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public static bool operator ==(Plant left, Plant right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Plant left, Plant right)
        {
            return !Equals(left, right);
        }

        public string Name { get; }

        #endregion

        #region All other members

        private bool Equals(Plant other)
        {
            return string.Equals(Name, other.Name);
        }

        #endregion
    }
}