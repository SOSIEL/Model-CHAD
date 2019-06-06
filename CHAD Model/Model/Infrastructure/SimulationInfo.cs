using System.Collections.Generic;

namespace CHAD.Model.Infrastructure
{
    public class SimulationInfo
    {
        #region Static Fields and Constants

        public const string DirectRunoff = "DirectRunoff (ac-in)";
        public const string EvapTransFromField = "EvapTransFromField (ac-in)";
        public const string EvapTransToDate = "EvapTransToDate (ac-in)";
        public const string IrrigNeed = "IrrigNeed (ac-in)";
        public const string IrrigOfField = "IrrigOfField (ac-in)";
        public const string IrrigSeason = "IrrigSeason (ac-in)";
        public const string LeakAquifer = "LeakAquifer (ac-ft)";
        public const string PercFromField = "PercFromField (ac-ft)";
        public const string Precip = "Precip (in)";
        public const string PrecipOnField = "PrecipOnField (ac-in)";
        public const string SnowInSnowpack = "SnowInSnowpack (in)";
        public const string WaterInAquifer = "WaterInAquifer (ac-ft)";
        public const string WaterInAquifer2 = "WaterInAquifer 2 (ac-ft)";
        public const string WaterInAquifer3 = "WaterInAquifer (ac-ft)";
        public const string WaterInAquiferChange = "WaterInAquiferChange (ac-ft)";
        public const string WaterInField = "WaterInField (ac-ft)";
        public const string WaterInField2 = "WaterInField 2 (ac-ft)";
        public const string WaterInField3 = "WaterInField 3 (ac-ft)";
        public const string WaterInput = "WaterInput (ac-in)";
        public const string Plant = "Plant";
        public const string Harvestable = "Harvestable";

        #endregion

        #region Constructors

        public SimulationInfo(string configurationName, string simulationSession, int simulationNumber, int seasonCount,
            int daysCount, IEnumerable<string> fieldNames)
        {
            ConfigurationName = configurationName;
            SimulationSession = simulationSession;
            SimulationNumber = simulationNumber;
            SeasonCount = seasonCount;
            DaysCount = daysCount;

            FieldNames = new List<string>(fieldNames);
            FieldRecordNames = new List<string>
            {
                Plant,
                Precip,
                PrecipOnField,
                SnowInSnowpack,
                IrrigNeed,
                IrrigOfField,
                IrrigSeason,
                DirectRunoff,
                WaterInput,
                WaterInField,
                EvapTransFromField,
                WaterInField2,
                WaterInAquifer,
                PercFromField,
                WaterInAquifer2,
                WaterInField3,
                EvapTransToDate,
                Harvestable
            };

            RecordNames = new List<string> {LeakAquifer, WaterInAquifer3, WaterInAquiferChange};
        }

        #endregion

        #region Public Interface

        public string ConfigurationName { get; }

        public int DaysCount { get; }

        public List<string> FieldNames { get; }

        public List<string> FieldRecordNames { get; }

        public List<string> RecordNames { get; }

        public int SeasonCount { get; }

        public int SimulationNumber { get; }

        public string SimulationSession { get; }

        #endregion
    }
}