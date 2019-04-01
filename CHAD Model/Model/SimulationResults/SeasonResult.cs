using System.Collections.Generic;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.RVACModule;

namespace CHAD.Model.SimulationResults
{
    public class SeasonResult
    {
        #region Constructors

        public SeasonResult(int number, double waterCurtailmentRate, Climate climate, AgroHydrology agroHydrology, RVAC rvac)
        {
            Number = number;
            WaterCurtailmentRate = waterCurtailmentRate;
            ClimateResult = new ClimateResult(climate);
            DailyHydrology = new List<DailyHydrology>(agroHydrology.DailyHydrology);
            RVACResult = new RVACResult(rvac);
        }

        #endregion

        #region Public Interface

        public double WaterCurtailmentRate { get; }

        public List<DailyHydrology> DailyHydrology { get; }

        public ClimateResult ClimateResult { get; }

        public int Number { get; }

        public RVACResult RVACResult { get; }

        #endregion
    }
}