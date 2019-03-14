using System.Collections.Generic;

namespace Model
{
    public class Configuration
    {
        #region Constructors

        public Configuration()
        {
            Parameters = new Parameters();
            ClimateList = new List<InputClimate>();
            CropEvapTransList = new List<InputCropEvapTrans>();
            FieldSizeList = new List<InputFieldSize>();
        }

        public Configuration(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Public Interface

        public string Name { get; set; }

        public int SeasonsCount { get; set; }

        public int DaysInSeasonCount { get; set; }

        public Parameters Parameters { get; }

        public List<InputClimate> ClimateList { get; }

        public List<InputCropEvapTrans> CropEvapTransList { get; }

        public List<InputFieldSize> FieldSizeList { get; }

        #endregion
    }
}