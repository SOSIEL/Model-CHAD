using System;
using System.Collections.Generic;

namespace Model.ClimateModule
{
    public class SeasonClimate
    {
        #region Constructors

        public SeasonClimate(IEnumerable<ClimateForecast> climateForecasts)
        {
            foreach (var climateForecast in climateForecasts)
            {
                var temperature = Gaussian(climateForecast.TempMean, climateForecast.TempSD);
                var precipitation = Gaussian(climateForecast.PrecipMean, climateForecast.PrecipSD);

                DailyClimate.Add(new DailyClimate(climateForecast.Day, temperature, precipitation));
            }
        }

        #endregion

        #region Public Interface

        public List<DailyClimate> DailyClimate { get; }

        #endregion

        #region All other members

        private static decimal Gaussian(double mean, double stddev)
        {
            var random = new Random();
            var x1 = 1 - random.NextDouble();
            var x2 = 1 - random.NextDouble();
            var y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return (decimal) Math.Round(y1 * stddev + mean, 2);
        }

        #endregion
    }
}