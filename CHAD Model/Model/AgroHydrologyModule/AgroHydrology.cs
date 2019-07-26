using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CHAD.Model.ClimateModule;
using CHAD.Model.Infrastructure;

namespace CHAD.Model.AgroHydrologyModule
{
    public class AgroHydrology
    {
        #region Static Fields and Constants

        private const double AcInToAcFt = 1d / 12;

        private const double RainToSnow = 10;

        private const int Beta = 2;

        #endregion

        #region Fields

        private readonly List<CropEvapTrans> _cropEvapTrans;
        private readonly List<Field> _fields;
        private readonly ILogger _logger;
        private readonly IAgroHydrologyCalculationLogger _calculationLogger;
        private readonly Parameters _parameters;

        private readonly Dictionary<Field, double> DirectRunoff;
        private readonly Dictionary<Field, double> EvapTransFromField;

        private readonly Dictionary<Field, double> EvapTransFromFieldSeasonMax;
        private readonly Dictionary<Field, double> EvapTransFromFieldToDate;
        private readonly Dictionary<Field, double> IrrigNeed;
        private readonly Dictionary<Field, double> IrrigOfField;
        private readonly Dictionary<Field, double> PercFromField;
        private readonly Dictionary<Field, double> PrecipOnField;
        private readonly Dictionary<Field, double> WaterInField;
        private readonly Dictionary<Field, double> WaterInFieldMax;
        private readonly Dictionary<Field, double> WaterInput;
        private double IrrigSeason;
        private double LeakAquifer;
        private double WaterInAquiferChange;

        private double WaterInAquiferPrior;
        private double WaterInSnowpack;
        private double WaterUsageMax;
        private double WaterUsageRemain;

        #endregion

        #region Constructors

        public AgroHydrology(ILogger logger, IAgroHydrologyCalculationLogger agroHydrologyCalculationLogger, Parameters parameters, List<Field> fields,
            List<CropEvapTrans> cropEvapTrans)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _calculationLogger = agroHydrologyCalculationLogger;
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            _cropEvapTrans = cropEvapTrans ?? throw new ArgumentNullException(nameof(CropEvapTrans));

            DailyHydrology = new List<DailyHydrology>();

            WaterInAquifer = _parameters.WaterInAquifer;
            WaterInSnowpack = _parameters.SnowInSnowpack;

            var fieldsCount = fields.Count;

            EvapTransFromField = new Dictionary<Field, double>(fieldsCount);
            EvapTransFromFieldToDate = new Dictionary<Field, double>(fieldsCount);
            EvapTransFromFieldSeasonMax = new Dictionary<Field, double>(fieldsCount);
            DirectRunoff = new Dictionary<Field, double>(fieldsCount);
            IrrigNeed = new Dictionary<Field, double>(fieldsCount);
            IrrigOfField = new Dictionary<Field, double>(fieldsCount);
            PercFromField = new Dictionary<Field, double>(fieldsCount);
            PrecipOnField = new Dictionary<Field, double>(fieldsCount);
            WaterInput = new Dictionary<Field, double>(fieldsCount);
            WaterInField = new Dictionary<Field, double>(fieldsCount);
            WaterInFieldMax = new Dictionary<Field, double>(fieldsCount);

            foreach (var field in fields)
            {
                EvapTransFromField[field] = 0;
                EvapTransFromFieldToDate[field] = 0;
                EvapTransFromFieldSeasonMax[field] = 0;
                DirectRunoff[field] = 0;
                IrrigNeed[field] = 0;
                IrrigOfField[field] = 0;
                PercFromField[field] = 0;
                PrecipOnField[field] = 0;
                WaterInput[field] = 0;
                WaterInField[field] = field.InitialWaterVolume;
                WaterInFieldMax[field] = _parameters.FieldDepth * field.FieldSize;
            }
        }

        #endregion

        #region Public Interface

        public void ProcessDay(int seasonNumber, int dayNumber, DailyClimate dailyClimate, List<FieldHistory> fieldHistories)
        {
            _logger.Write($"AgroHydrology: start the processing of day {dayNumber}", Severity.Level2);

            // Documented to calculate WaterInAquiferChange at the end of season.
            WaterInAquiferPrior = WaterInAquifer;
            _logger.Write($"WaterInAquifer = {WaterInAquiferPrior} at the beginning of the season", Severity.Level3);

            var Precip = dailyClimate.Precipitation;
            var MeltingRate = Math.Max(0, (dailyClimate.Temperature - _parameters.MeltingPoint) / 100);
            _calculationLogger.AddRecord(seasonNumber, dayNumber, SimulationInfo.Precipitation, Precip);

            if (dailyClimate.Temperature > _parameters.MeltingPoint)
            {
                _logger.Write("Temperature > MeltingPoint.", Severity.Level3);
                Precip = dailyClimate.Precipitation +
                                       MeltingRate * (WaterInSnowpack / RainToSnow);
                WaterInSnowpack = WaterInSnowpack - MeltingRate * WaterInSnowpack;
            }
            else
            {
                _logger.Write("Temperature <= MeltingPoint.", Severity.Level3);
                WaterInSnowpack = WaterInSnowpack + Precip * RainToSnow;
                Precip = 0;
            }

            WaterInSnowpack = Math.Round(WaterInSnowpack, 2);

            _calculationLogger.AddRecord(seasonNumber, dayNumber, SimulationInfo.Precip, Precip);
            _calculationLogger.AddRecord(seasonNumber, dayNumber, SimulationInfo.SnowInSnowpack, WaterInSnowpack);

            foreach (var fieldHistory in fieldHistories)
            {
                var field = fieldHistory.Field;
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.Plant, fieldHistory.Plant);
                _logger.Write($"Process field number {field.FieldNumber}", Severity.Level2);
                _logger.Write($"{fieldHistory.Plant} is planted on the field", Severity.Level3);

                PrecipOnField[field] = Precip * fieldHistory.Field.FieldSize;
                _logger.Write($"PrecipOnField = {PrecipOnField[field]}", Severity.Level3);
                _logger.Write($"WaterInSnowpack = {WaterInSnowpack}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.PrecipOnField, PrecipOnField[field]);

                if (fieldHistory.Plant == Plant.Nothing)
                    IrrigNeed[field] = 0;
                else
                    IrrigNeed[field] =
                        Math.Max(0, _cropEvapTrans.First(et => et.Day == dayNumber).GetEvapTrans(fieldHistory.Plant) *
                                    fieldHistory.Field.FieldSize -
                                    PrecipOnField[field]);
                _logger.Write($"IrrigNeed = {IrrigNeed[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.IrrigNeed, IrrigNeed[field]);

                WaterUsageRemain = Math.Max(0, WaterUsageMax - IrrigSeason);
                _logger.Write($"WaterUsageRemain = {WaterUsageRemain}", Severity.Level3);

                IrrigOfField[field] =
                    Math.Min(Math.Min(IrrigNeed[field], WaterUsageRemain), WaterInAquifer / AcInToAcFt);
                _logger.Write($"IrrigOfField = {IrrigOfField[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.IrrigOfField, IrrigOfField[field]);

                IrrigSeason = IrrigSeason + IrrigOfField[field];
                _logger.Write($"IrrigSeason = {IrrigSeason}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.IrrigSeason, IrrigSeason);

                DirectRunoff[field] = (PrecipOnField[field] + IrrigOfField[field]) * Math.Pow(WaterInField[field] / WaterInFieldMax[field], Beta);
                _logger.Write($"DirectRunoff = {DirectRunoff[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.DirectRunoff, DirectRunoff[field]);

                WaterInput[field] = PrecipOnField[field] + IrrigOfField[field] - DirectRunoff[field];
                _logger.Write($"WaterInput = {WaterInput[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.WaterInput, WaterInput[field]);

                WaterInField[field] = Math.Min(WaterInFieldMax[field], WaterInField[field] + AcInToAcFt * WaterInput[field]);
                _logger.Write($"WaterInField + AcInToFt3 * WaterInput = {WaterInField[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.WaterInField, WaterInField[field]);

                if (fieldHistory.Plant == Plant.Nothing)
                    EvapTransFromField[field] = 0;
                else
                    EvapTransFromField[field] =
                        Math.Min(_cropEvapTrans.First(et => et.Day == dayNumber).GetEvapTrans(fieldHistory.Plant) * fieldHistory.Field.FieldSize, 
                                    WaterInField[field] / AcInToAcFt);
                _logger.Write($"EvapTransFromField = {EvapTransFromField[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.EvapTransFromField, EvapTransFromField[field]);

                WaterInField[field] = WaterInField[field] - AcInToAcFt * EvapTransFromField[field];
                _logger.Write($"WaterInField - EvapTransFromField = {WaterInField[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.WaterInField2, WaterInField[field]);

                WaterInAquifer = WaterInAquifer - AcInToAcFt * IrrigOfField.Sum(i => i.Value);
                _logger.Write($"WaterInAquifer = {WaterInAquifer}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.WaterInAquifer, WaterInAquifer);

                PercFromField[field] = Math.Min(_parameters.PercFromFieldFrac * WaterInField[field],
                    _parameters.WaterInAquiferMax - WaterInAquifer);
                _logger.Write($"PercFromField = {PercFromField[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.PercFromField, PercFromField[field]);

                WaterInAquifer = WaterInAquifer + PercFromField[field];
                _logger.Write($"WaterInAquifer = WaterInAquifer + PercFromField = {WaterInAquifer}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.WaterInAquifer2, WaterInAquifer);

                WaterInField[field] = WaterInField[field] - PercFromField[field];
                _logger.Write($"WaterInField = WaterInField - PercFromField = {WaterInField[field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.WaterInField3, WaterInField[field]);

                EvapTransFromFieldToDate[fieldHistory.Field] = EvapTransFromFieldToDate[fieldHistory.Field] + EvapTransFromField[field];
                _logger.Write($"EvapTransFromFieldToDate = EvapTransFromFieldToDate + EvapTransFromField = {EvapTransFromFieldToDate[fieldHistory.Field]}", Severity.Level3);
                _calculationLogger.AddRecord(seasonNumber, dayNumber, fieldHistory.Field.FieldNumber.ToString(), SimulationInfo.EvapTransToDate, EvapTransFromFieldToDate[field]);
            }

            LeakAquifer = _parameters.LeakAquiferFrac * WaterInAquifer;
            _logger.Write($"LeakAquifer = {LeakAquifer}", Severity.Level3);
            _calculationLogger.AddRecord(seasonNumber, dayNumber, SimulationInfo.LeakAquifer, LeakAquifer);

            WaterInAquifer = WaterInAquifer - LeakAquifer;
            _logger.Write($"WaterInAquifer = {WaterInAquifer}", Severity.Level3);
            _calculationLogger.AddRecord(seasonNumber, dayNumber, SimulationInfo.WaterInAquifer3, WaterInAquifer);

            WaterInAquiferChange = WaterInAquifer - WaterInAquiferPrior;
            _logger.Write($"WaterInAquiferChange = {WaterInAquiferChange}", Severity.Level3);
            _calculationLogger.AddRecord(seasonNumber, dayNumber, SimulationInfo.WaterInAquiferChange, WaterInAquiferChange);

            DailyHydrology.Add(new DailyHydrology(dayNumber, WaterInAquifer, WaterInSnowpack));

            _logger.Write($"AgroHydrology: finish the processing of day {dayNumber}", Severity.Level2);
        }

        public void ProcessSeasonEnd(int seasonNumber, List<FieldHistory> fieldHistories)
        {
            _logger.Write("AgroHydrology: start the processing of season result", Severity.Level2);

            double alfalfaSumToDate = 0;
            double alfalfaSumMax = 0;
            double barleySumToDate = 0;
            double barleySumMax = 0;
            double wheatSumToDate = 0;
            double wheatSumMax = 0;

            foreach (var fieldHistory in fieldHistories)
            {
                if (fieldHistory.Plant == Plant.Alfalfa)
                {
                    alfalfaSumToDate += EvapTransFromFieldToDate[fieldHistory.Field];
                    alfalfaSumMax += EvapTransFromFieldSeasonMax[fieldHistory.Field];
                }

                if (fieldHistory.Plant == Plant.Barley)
                {
                    barleySumToDate += EvapTransFromFieldToDate[fieldHistory.Field];
                    barleySumMax += EvapTransFromFieldSeasonMax[fieldHistory.Field];
                }

                if (fieldHistory.Plant == Plant.Wheat)
                {
                    wheatSumToDate += EvapTransFromFieldToDate[fieldHistory.Field];
                    wheatSumMax += EvapTransFromFieldSeasonMax[fieldHistory.Field];
                }
            }

            HarvestableAlfalfa = alfalfaSumMax != 0 ? alfalfaSumToDate / alfalfaSumMax : HarvestableAlfalfa;
            HarvestableBarley = barleySumMax != 0 ? barleySumToDate / barleySumMax : HarvestableBarley;
            HarvestableWheat = wheatSumMax != 0 ? wheatSumToDate / wheatSumMax : HarvestableWheat;

            _calculationLogger.AddRecord(seasonNumber, SimulationInfo.HarvestableAlfalfa, HarvestableAlfalfa);
            _calculationLogger.AddRecord(seasonNumber, SimulationInfo.HarvestableBarley, HarvestableBarley);
            _calculationLogger.AddRecord(seasonNumber, SimulationInfo.HarvestableWheat, HarvestableWheat);

            _logger.Write($"HarvestableAlfalfa = {HarvestableAlfalfa}", Severity.Level3);
            _logger.Write($"HarvestableBarley = {HarvestableBarley}", Severity.Level3);
            _logger.Write($"HarvestableWheat = {HarvestableWheat}", Severity.Level3);

            _logger.Write("AgroHydrology: finish the processing of season result", Severity.Level2);
        }

        public void ProcessSeasonStart(List<FieldHistory> fieldHistories, double waterUseRedFrac)
        {
            _logger.Write("AgroHydrology: start the processing of season preparation", Severity.Level2);

            IrrigSeason = 0;

            foreach (var field in _fields)
            {
                EvapTransFromFieldToDate[field] = 0;
            }
           
            _logger.Write($"WaterUseRedFrac = {waterUseRedFrac}", Severity.Level3);

            WaterUsageMax = _parameters.WaterUseBase * (1 - waterUseRedFrac);
            _logger.Write($"WaterUsageMax = {WaterUsageMax} for current season", Severity.Level3);

            // The maximum amount of water that can evaporate from field when accounting for plant type and field size.
            foreach (var fieldHistory in fieldHistories.Where(fh => fh.Plant != Plant.Nothing))
            {
                EvapTransFromFieldSeasonMax[fieldHistory.Field] =
                    _cropEvapTrans.Select(et => et.GetEvapTrans(fieldHistory.Plant)).Sum() *
                    fieldHistory.Field.FieldSize;
                _logger.Write(
                    $"EvapTransFromFieldSeasonMax = {EvapTransFromFieldSeasonMax[fieldHistory.Field]} for field {fieldHistory.Field.FieldNumber}",
                    Severity.Level3);
            }

            _logger.Write("AgroHydrology: finish the processing of season preparation", Severity.Level2);
        }

        public List<DailyHydrology> DailyHydrology { get; }

        public double HarvestableAlfalfa { get; private set; }

        public double HarvestableBarley { get; private set; }

        public double HarvestableWheat { get; private set; }

        public double WaterInAquifer { get; private set; }

        #endregion
    }
}