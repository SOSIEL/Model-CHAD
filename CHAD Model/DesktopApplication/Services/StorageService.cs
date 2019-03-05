using System.Collections.Generic;
using DesktopApplication.Models;

namespace DesktopApplication.Services
{
    public class StorageService : IStorageService
    {
        public IEnumerable<Simulation> GetSimulations()
        {
           return new List<Simulation>
           {
               new Simulation("Simulation1"),
               new Simulation("Simulation2")
           };
        }
    }
}
