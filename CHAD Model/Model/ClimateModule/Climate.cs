using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CHAD.Model.ClimateModule
{
    public class Climate : IEnumerable<DailyClimate>
    {
        #region Fields

        private readonly Parameters _parameters;
        private readonly IEnumerable<ClimateForecast> _climateForecasts;

        private readonly List<DailyClimate> _dailyClimates;

        #endregion

        #region Constructors

        public Climate(Parameters parameters, IEnumerable<ClimateForecast> climateForecasts)
        {
            _parameters = parameters;
            _climateForecasts = climateForecasts;
            _dailyClimates = new List<DailyClimate>();
        }

        #endregion

        #region Public Interface

        public void ProcessSeason(int seasonNumber)
        {
            _dailyClimates.Clear();

            foreach (var climateForecast in _climateForecasts)
            {
                var temperature = Gaussian(seasonNumber,
                    climateForecast.TempMean,
                    climateForecast.TempSD,
                    _parameters.ClimateChangeTempMean,
                    _parameters.ClimateChangeTempSD);

                var precipitation = Gaussian(seasonNumber,
                    climateForecast.PrecipMean,
                    climateForecast.PrecipSD,
                    _parameters.ClimateChangePrecipMean,
                    _parameters.ClimateChangePrecipSD);

                _dailyClimates.Add(new DailyClimate(climateForecast.Day, temperature, precipitation));
            }
        }

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

        private double Gaussian(int seasonNumber, double mean, double standardDeviation, double changeMean, double changeDeviation)
        {
            var random = new Random();
            var x1 = 1 - random.NextDouble();
            var x2 = 1 - random.NextDouble();
            var y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return Math.Round(y1 * standardDeviation * Math.Pow(changeDeviation, seasonNumber) + mean * Math.Pow(changeMean, seasonNumber), 2);
        }

        #endregion
    }
}