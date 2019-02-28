using System;
using System.Configuration;

namespace ConsoleApp10
{
    #region EXCEL OBJECTS
    public class InputClimate
    {
        public int t { get; set; }
        public double TempMean { get; set; } 

        public double TempSD { get; set; } 
        public double PrecipMean { get; set; } 
        public double PrecipSD { get; set; } 

        public decimal TempMeanRandom { get; set; }
        public decimal PrecipMeanRandom { get; set; }
    }

    public class InputCropEvapTrans
    {
        public int t { get; set; }
        public int CropType { get; set; }
        public String CropName { get; set; }
        public decimal Quantity { get; set; }
    }

    public class InputFieldSize
    {
        public decimal FieldNum { get; set; }
        public decimal FieldSize { get; set; }
        public decimal WaterInAquifer { get; set; }
    }
    #endregion

    public class Hydrology
    {
        public int Day { get; set; }
        public int Field { get; set; }
        public decimal WaterInAquifer { get; set; }
        public decimal WaterInField { get; set; }
    }

    static class Configuration
    {
        public static decimal ToDecimal(String input)
        {
            char separator = input.Contains(".") ? '.' : ',';
            char newSeparator = !input.Contains(".") ? '.' : ',';
            decimal result;
            if (Decimal.TryParse(input, out result))
                return result;
            return Convert.ToDecimal(input.Replace(separator, newSeparator));
        }

        // The amount of water in the aquifer (in gallons).
        public static decimal WaterInAquifer { get; } = ToDecimal(ConfigurationManager.AppSettings["WaterInAquifer"]);

        // The maximum amount of water the aquifer can hold (in gallons).
        public static decimal WaterInAquiferMax { get; } = ToDecimal(ConfigurationManager.AppSettings["WaterInAquiferMax"]);

        // The exponent in the equation calculating the direct runoff from a field.
        public static decimal Beta { get; } = ToDecimal(ConfigurationManager.AppSettings["Beta"]);

        // The number of seasons to be simulated
        public static decimal NumOfSeasons { get; } = ToDecimal(ConfigurationManager.AppSettings["NumOfSeasons"]);

        // The fraction of water in the aquifer that is lost through leakage.
        public static decimal LeakAquiferFrac { get; } = ToDecimal(ConfigurationManager.AppSettings["LeakAquiferFrac"]);

        // The fraction of water in a field that flows into the aquifer (in gallons). The fraction is the same for all fields.
        public static decimal PercFromFieldFrac { get; } = ToDecimal(ConfigurationManager.AppSettings["PercFromFieldFrac"]);

        // The coefficient that in combination with a field’s size (in acres) determines its maximum capacity to store water (in acre-inches).

        public static decimal WaterStorCap { get; } = ToDecimal(ConfigurationManager.AppSettings["WaterStorCap"]);
    }
}