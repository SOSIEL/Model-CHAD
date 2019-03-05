using System.Collections.Generic;
using DesktopApplication.Models;

namespace DesktopApplication.Services
{
    public interface IStorageService
    {
        IEnumerable<Simulation> GetSimulations();
    }
}
