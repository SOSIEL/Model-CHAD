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
            ClimateList = new List<ClimateForecast>();
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

        public int DaysCount => ClimateList.Count;

        public Parameters Parameters { get; set; }

        public List<ClimateForecast> ClimateList { get; }

        public List<InputCropEvapTrans> CropEvapTransList { get; }

        public List<Field> Fields { get; }

        #endregion
    }
}