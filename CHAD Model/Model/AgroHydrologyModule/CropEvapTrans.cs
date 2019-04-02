using System;

namespace CHAD.Model.AgroHydrologyModule
{
    public class CropEvapTrans
    {
        #region Constructors

        public CropEvapTrans(int day, double alfalfaValue, double barleyValue, double wheatValue)
        {
            Day = day;
            AlfalfaValue = alfalfaValue;
            BarleyValue = barleyValue;
            WheatValue = wheatValue;
        }

        #endregion

        #region Public Interface

        public double GetEvapTrans(Plant plant)
        {
            switch (plant)
            {
                case Plant.Alfalfa:
                    return AlfalfaValue;
                case Plant.Barley:
                    return BarleyValue;
                case Plant.Wheat:
                    return WheatValue;
            }

            throw new ArgumentOutOfRangeException(nameof(plant));
        }

        public double AlfalfaValue { get; }

        public double BarleyValue { get; }

        public int Day { get; set; }

        public double WheatValue { get; }

        #endregion
    }
}