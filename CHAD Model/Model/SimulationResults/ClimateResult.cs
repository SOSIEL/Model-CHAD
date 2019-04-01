using System.Collections.Generic;
using CHAD.Model.ClimateModule;

namespace CHAD.Model.SimulationResults
{
    public struct ClimateResult
    {
        public ClimateResult(IEnumerable<DailyClimate> dailyClimate)
        {
            DailyClimate = new List<DailyClimate>(dailyClimate);
        }

        public List<DailyClimate> DailyClimate { get; }
    }
}