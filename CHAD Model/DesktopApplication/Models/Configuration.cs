namespace DesktopApplication.Models
{
    public class Configuration
    {
        #region Constructors

        public Configuration()
        {
        }

        public Configuration(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties, Indexers

        public string Name { get; set; }

        #endregion
    }
}