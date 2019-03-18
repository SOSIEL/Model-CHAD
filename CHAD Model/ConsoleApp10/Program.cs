using System;
using System.Configuration;
using System.IO;
using DataAccess;
using Model;

namespace ConsoleApp10
{
    public static class Program
    {
        #region Static Fields and Constants

        private const string ConfigurationPath = "Configuration";

        #endregion

        #region Public Interface

        public static void Main()
        {
            var storageService = new FileStorageService();

            var configuration = storageService.GetConfiguration("ConsoleApplicationConfiguration",
                Path.Combine(Directory.GetCurrentDirectory(), ConfigurationPath));

            var logger = new SimpleLogger();

            configuration.Parameters.NumOfSeasons = 1;

            var simulator = new Simulator(logger);
            simulator.SetConfiguration(configuration);
            simulator.Start();

            var date = DateTime.Now;
            var path = ConfigurationManager.AppSettings["filepath"];
            path = path + "/" + date.ToString("dd_MM_yyyy_HH_mm_ss_fff");

            storageService.SaveLogs(path, logger);
            storageService.SaveHydrology(path, simulator.AgroHydrology.Hydrology, configuration.Fields);
            storageService.SaveClimate(path, simulator.AgroHydrology.ClimateList);
        }

        #endregion
    }
}