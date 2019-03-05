using System.Collections.Generic;
using DesktopApplication.Models;

namespace DesktopApplication.Services
{
    public interface IStorageService
    {
        #region All other members

        IEnumerable<Configuration> GetSimulations();

        #endregion
    }
}