using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CHAD.Model.ClimateModule
{
    public class Climate : IEnumerable<DailyClimate>
    {
        #region Fields

        private readonly List<DailyClimate> _dailyClimates;

        #endregion

        #region Constructors

        public Climate(IEnumerable<ClimateForecast> climateForecasts)
        {
            _dailyClimates = new List<DailyClimate>();

            foreach (var climateForecast in climateForecasts)
            {
                var temperature = Gaussian(climateForecast.TempMean, climateForecast.TempSD);
                var precipitation = Gaussian(climateForecast.PrecipMean, climateForecast.PrecipSD);

                _dailyClimates.Add(new DailyClimate(climateForecast.Day, temperature, precipitation));
            }
        }

        #endregion

        #region Public Interface

        public DailyClimate GetDailyClimate(int day)
        {
            return _dailyClimates.FirstOrDefault(dc => dc.Day == day);
        }

        public IEnumerator<DailyClimate> GetEnumerator()
        {
            return _dailyClimates.GetEnumerator();
        }

        #endregion

        #region Interface Implementations

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region All other members

        private static decimal Gaussian(double mean, double standardDeviation)
        {
            var random = new Random();
            var x1 = 1 - random.NextDouble();
            var x2 = 1 - random.NextDouble();
            var y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return (decimal) Math.Round(y1 * standardDeviation + mean, 2);
        }

        #endregion
    }
}