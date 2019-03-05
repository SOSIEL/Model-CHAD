using System.Collections.Generic;
using DesktopApplication.Models;

namespace DesktopApplication.Services
{
    public class StorageService : IStorageService
    {
        #region Interface Implementations

        public IEnumerable<Configuration> GetSimulations()
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