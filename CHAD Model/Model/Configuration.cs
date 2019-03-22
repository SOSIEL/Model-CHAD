using System.Collections.Generic;
using Model.ClimateModule;

namespace Model
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
        }

        public Configuration(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Public Interface

        public string Name { get; set; }

        public int DaysCount => ClimateForecast.Count;

        public Parameters Parameters { get; set; }

        public List<ClimateForecast> ClimateForecast { get; }

        public List<InputCropEvapTrans> CropEvapTransList { get; }

        public List<Field> Fields { get; }

        #endregion
    }
}