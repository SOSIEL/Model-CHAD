using System;
using System.Collections.Generic;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.RVACModule;
using CHADSOSIEL.Configuration;

namespace CHAD.Model
{
    public class Configuration
    {
        #region Constructors

        public Configuration()
        {
            Parameters = new Parameters();
            ClimateForecast = new List<ClimateForecast>();
            CropEvapTransList = new List<InputCropEvapTrans>();
            Fields = new List<Field>();
            MarketPrices = new List<MarketPrice>();
        }

        public Configuration(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Public Interface

        public List<ClimateForecast> ClimateForecast { get; }

        public List<InputCropEvapTrans> CropEvapTransList { get; }

        public int DaysCount => ClimateForecast.Count;

        public List<Field> Fields { get; }

        public List<MarketPrice> MarketPrices { get; }

        public string Name { get; set; }

        public Parameters Parameters { get; set; }

        public ConfigurationModel SOSIELConfiguration { get; set; }

        #endregion
    }
}