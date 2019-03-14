using System.Collections.Generic;
using Model;

namespace DesktopApplication.Services
{
    public interface IStorageService
    {
        #region All other members

        IEnumerable<Configuration> GetConfigurations();

        #endregion
    }
}