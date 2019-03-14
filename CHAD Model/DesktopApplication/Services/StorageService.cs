using System.Collections.Generic;
using Model;

namespace DesktopApplication.Services
{
    public class StorageService : IStorageService
    {
        #region Interface Implementations

        public IEnumerable<Configuration> GetConfigurations()
        {
            return new List<Configuration>
            {
                new Configuration("Configuration1"),
                new Configuration("Configuration2")
            };
        }

        #endregion
    }
}