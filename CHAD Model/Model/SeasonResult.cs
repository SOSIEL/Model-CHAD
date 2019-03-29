using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.RVACModule;

namespace CHAD.Model
{
    public class SeasonResult
    {
        #region Constructors

        public SeasonResult(int number, double waterCurtailmentRate, Climate climate, AgroHydrology agroHydrology, RVAC rvac)
        {
            Number = number;
            WaterCurtailmentRate = waterCurtailmentRate;
            Climate = climate;
            AgroHydrology = agroHydrology;
            RVAC = rvac;
        }

        #endregion

        #region Public Interface

        public double WaterCurtailmentRate { get; }

        public AgroHydrology AgroHydrology { get; }

        public Climate Climate { get; }

        public int Number { get; }

        public RVAC RVAC { get; }

        #endregion
    }
}