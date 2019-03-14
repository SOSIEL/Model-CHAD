namespace Model
{
    public class Configuration
    {
        #region Constructors

        public Configuration()
        {
            Parameters = new Parameters();
        }

        public Configuration(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Public Members

        public string Name { get; set; }

        public Parameters Parameters { get; }

        #endregion
    }
}