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
        private readonly List<DroughtLevel> _droughtLevels;


        #endregion

        #region Constructors

        public Climate(Parameters parameters, IEnumerable<ClimateForecast> climateForecasts, IEnumerable<DroughtLevel> droughtLevel)
        {
            _parameters = parameters;
            _climateForecasts = climateForecasts;
            _dailyClimates = new List<DailyClimate>();
            _droughtLevels = new List<DroughtLevel>(droughtLevel);
        }

        #endregion

        #region Public Interface

        public void ProcessSeason(int seasonNumber)
        {
            _dailyClimates.Clear();

            foreach (var climateForecast in _climateForecasts)
            {
                var temperature = GetRandomTemperature(seasonNumber, climateForecast);

                var precipitation = GetRandomPrecipitation(seasonNumber, climateForecast);

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

        private double GetDroughtLevel(int seasonNumber)
        {
            return _droughtLevels.First(dl => dl.SeasonNumber == seasonNumber).Value;
        }

        private double GetRandomTemperature(int seasonNumber, ClimateForecast climateForecast)
        {
            var random = new Random();
            var x1 = 1 - random.NextDouble();
            var x2 = 1 - random.NextDouble();
            var y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);

            var result = y1 * climateForecast.TempSD * Math.Pow(_parameters.ClimateChangeTempSD, seasonNumber) +
                         climateForecast.TempMean * Math.Pow(_parameters.ClimateChangeTempMean, seasonNumber);

            return Math.Round(result, 2);
        }

        private double GetRandomPrecipitation(int seasonNumber, ClimateForecast climateForecast)
        {
            var random = new Random();
            var x1 = 1 - random.NextDouble();
            var x2 = 1 - random.NextDouble();
            var y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);

            var result = y1 * climateForecast.PrecipSD * Math.Pow(_parameters.ClimateChangePrecipSD, seasonNumber) +
                         GetDroughtLevel(seasonNumber) * climateForecast.PrecipMean *
                         Math.Pow(_parameters.ClimateChangePrecipMean, seasonNumber);

            return Math.Round(result, 2);
        }

        #endregion
    }
}