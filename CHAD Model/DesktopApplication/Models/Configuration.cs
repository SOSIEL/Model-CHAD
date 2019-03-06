namespace DesktopApplication.Models
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

        #region Properties, Indexers

        public string Name { get; set; }

        public Parameters Parameters { get; }

        #endregion
    }
}