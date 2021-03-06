﻿using System.Collections.Generic;
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
            DroughtLevels = new List<DroughtLevel>();
            CropEvapTransList = new List<CropEvapTrans>();
            Fields = new List<Field>();
            MarketPrices = new List<MarketPrice>();
            AvailableSosielConfigurations = new List<string>();
        }

        public Configuration(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Public Interface

        public List<ClimateForecast> ClimateForecast { get; }

        public List<CropEvapTrans> CropEvapTransList { get; }

        public List<DroughtLevel> DroughtLevels { get; }

        public List<Field> Fields { get; }

        public List<MarketPrice> MarketPrices { get; }

        public string Name { get; set; }

        public Parameters Parameters { get; set; }

        public ConfigurationModel SOSIELConfiguration { get; set; }

        public List<string> AvailableSosielConfigurations { get; set; }

        #endregion
    }
}