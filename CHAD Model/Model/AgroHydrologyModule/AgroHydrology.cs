using System;
using System.Collections.Generic;
using System.Linq;
using CHAD.Model.ClimateModule;
using CHAD.Model.Infrastructure;

namespace CHAD.Model.AgroHydrologyModule
{
    public class AgroHydrology
    {
        #region Fields

        private readonly List<CropEvapTrans> _cropEvapTrans;
        private readonly ILogger _logger;
        private readonly Parameters _parameters;
        private readonly List<Field> _fields;

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
        public double WaterInAquifer { get; private set; }
        private double WaterInAquiferChange;

        private double WaterInAquiferPrior;
        private double WaterInSnowpack;
        private double WaterUsageMax;
        private double WaterUsageRemain;

        #endregion

        #region Constructors

        public AgroHydrology(ILogger logger, Parameters parameters, List<Field> fields,
            List<CropEvapTrans> cropEvapTrans)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            _cropEvapTrans = cropEvapTrans ?? throw new ArgumentNullException(nameof(CropEvapTrans));

            DailyHydrology = new List<DailyHydrology>();

            WaterInAquifer = _parameters.WaterInAquifer;
            WaterInSnowpack = _parameters.WaterInSnowpack;

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
                WaterInField[field] = 0;
                WaterInFieldMax[field] = Math.Round(_parameters.FieldDepth * field.FieldSize, 2);
            }

            HarvestableAlfalfa = 1;
            HarvestableBarley = 1;
            HarvestableWheat = 1;
        }

        #endregion

        #region Public Interface

        public List<DailyHydrology> DailyHydrology { get; }

        public double HarvestableAlfalfa { get; private set; }

        public double HarvestableBarley { get; private set; }

        public double HarvestableWheat { get; private set; }

        public void ProcessDay(int dayNumber, DailyClimate dailyClimate, List<FieldHistory> fieldHistories)
        {
            _logger.Write($"AgroHydrology: start the processing of day {dayNumber}", Severity.Level2);

            foreach (var fieldHistory in fieldHistories)
            {
                var field = fieldHistory.Field;
                _logger.Write($"Process field number {field.FieldNumber}", Severity.Level2);
                _logger.Write($"{fieldHistory.Plant} is planted on the field", Severity.Level3);

                PrecipOnField[field] = dailyClimate.Precipitation;

                var MeltingRate = Math.Max(0, (dailyClimate.Temperature - _parameters.MeltingPoint) / 100);

                if (dailyClimate.Temperature > _parameters.MeltingPoint)
                {
                    _logger.Write("Temperature > MeltingPoint.", Severity.Level3);
                    PrecipOnField[field] = dailyClimate.Precipitation +
                                           MeltingRate * (WaterInSnowpack / RainToSnow);
                    WaterInSnowpack = WaterInSnowpack - MeltingRate * WaterInSnowpack;
                }
                else
                {
                    _logger.Write("Temperature <= MeltingPoint.", Severity.Level3);
                    WaterInSnowpack = WaterInSnowpack + PrecipOnField[field] * RainToSnow;
                    PrecipOnField[field] = 0;
                }

                PrecipOnField[field] = PrecipOnField[field] * fieldHistory.Field.FieldSize;
                _logger.Write($"PrecipOnField = {PrecipOnField[field]}", Severity.Level3);
                _logger.Write($"WaterInSnowpack = {WaterInSnowpack}", Severity.Level3);

                if (fieldHistory.Plant == Plant.Nothing)
                    IrrigNeed[field] = 0;
                else
                    IrrigNeed[field] =
                        Math.Max(0, _cropEvapTrans.First(et => et.Day == dayNumber).GetEvapTrans(fieldHistory.Plant) *
                                    fieldHistory.Field.FieldSize -
                                    PrecipOnField[field]);

                _logger.Write($"IrrigNeed = {IrrigNeed[field]}", Severity.Level3);

                WaterUsageRemain = Math.Max(0, WaterUsageMax - IrrigSeason);
                _logger.Write($"WaterUsageRemain = {WaterUsageRemain}", Severity.Level3);

                IrrigOfField[field] = Math.Min(Math.Min(IrrigNeed[field], WaterUsageRemain), WaterInAquifer / AcInToFt3);
                _logger.Write($"IrrigOfField = {IrrigOfField[field]}", Severity.Level3);

                IrrigSeason = IrrigSeason + IrrigOfField[field];
                _logger.Write($"IrrigSeason = {IrrigSeason}", Severity.Level3);

                DirectRunoff[field] = (PrecipOnField[field] + IrrigOfField[field]) *
                                      Math.Pow(WaterInField[field] / WaterInFieldMax[field], _parameters.Beta);
                _logger.Write($"DirectRunoff = {DirectRunoff[field]}", Severity.Level3);

                WaterInput[field] = PrecipOnField[field] + IrrigOfField[field] - DirectRunoff[field];
                _logger.Write($"WaterInput = {WaterInput[field]}", Severity.Level3);

                WaterInField[field] = WaterInField[field] + AcInToFt3 * WaterInput[field];
                _logger.Write($"WaterInField + AcInToFt3 * WaterInput = {WaterInField[field]}", Severity.Level3);

                if (fieldHistory.Plant == Plant.Nothing)
                    EvapTransFromField[field] = 0;
                else
                    EvapTransFromField[field] =
                        Math.Min(
                            _cropEvapTrans.First(et => et.Day == dayNumber).GetEvapTrans(fieldHistory.Plant) *
                            fieldHistory.Field.FieldSize, WaterInField[field] / AcInToFt3);
                _logger.Write($"EvapTransFromField = {EvapTransFromField[field]}", Severity.Level3);

                WaterInField[field] = WaterInField[field] - EvapTransFromField[field];
                _logger.Write($"WaterInField - EvapTransFromField = {WaterInField[field]}", Severity.Level3);

                WaterInAquifer = WaterInAquifer - AcInToFt3 * IrrigOfField.Sum(i => i.Value);
                _logger.Write($"WaterInAquifer = {WaterInAquifer}", Severity.Level3);

                PercFromField[field] = Math.Min(_parameters.PercFromFieldFrac * WaterInField[field],
                    _parameters.WaterInAquiferMax - WaterInAquifer);
                _logger.Write($"PercFromField = {PercFromField[field]}", Severity.Level3);

                WaterInAquifer = WaterInAquifer + PercFromField[field];
                _logger.Write($"WaterInAquifer = WaterInAquifer + PercFromField = {WaterInAquifer}", Severity.Level3);

                WaterInField[field] = WaterInField[field] - PercFromField[field];
                _logger.Write($"WaterInField = WaterInField - PercFromField = {WaterInField[field]}", Severity.Level3);

                EvapTransFromFieldToDate[fieldHistory.Field] = EvapTransFromFieldToDate[fieldHistory.Field] + EvapTransFromField[field];
                _logger.Write($"EvapTransFromFieldToDate = EvapTransFromFieldToDate + EvapTransFromField = {EvapTransFromFieldToDate[fieldHistory.Field]}", Severity.Level3);
            }

            LeakAquifer = _parameters.LeakAquiferFrac * WaterInAquifer;
            _logger.Write($"LeakAquifer = {LeakAquifer}", Severity.Level3);

            WaterInAquifer = WaterInAquifer - LeakAquifer;
            _logger.Write($"WaterInAquifer = {WaterInAquifer}", Severity.Level3);

            WaterInAquiferChange = WaterInAquifer - WaterInAquiferPrior;
            _logger.Write($"WaterInAquiferChange = {WaterInAquiferChange}", Severity.Level3);

            DailyHydrology.Add(new DailyHydrology(dayNumber, WaterInAquifer, WaterInSnowpack));

            _logger.Write($"AgroHydrology: finish the processing of day {dayNumber}", Severity.Level2);
        }

        public void ProcessSeasonEnd(List<FieldHistory> fieldHistories)
        {
            _logger.Write($"AgroHydrology: start the processing of season result", Severity.Level2);

            foreach (var fieldHistory in fieldHistories)
            {
                var field = fieldHistory.Field;

                if (fieldHistory.Plant == Plant.Alfalfa)
                    HarvestableAlfalfa = HarvestableAlfalfa + EvapTransFromFieldToDate[field] /
                                         EvapTransFromFieldSeasonMax[field];

                if (fieldHistory.Plant == Plant.Barley)
                    HarvestableBarley = HarvestableBarley + EvapTransFromFieldToDate[field] /
                                        EvapTransFromFieldSeasonMax[field];

                if (fieldHistory.Plant == Plant.Wheat)
                    HarvestableWheat = HarvestableWheat + EvapTransFromFieldToDate[field] /
                                       EvapTransFromFieldSeasonMax[field];
            }

            _logger.Write($"HarvestableAlfalfa = {HarvestableAlfalfa}", Severity.Level3);
            _logger.Write($"HarvestableBarley = {HarvestableBarley}", Severity.Level3);
            _logger.Write($"HarvestableWheat = {HarvestableWheat}", Severity.Level3);

            _logger.Write($"AgroHydrology: finish the processing of season result", Severity.Level2);
        }

        public void ProcessSeasonStart(List<FieldHistory> fieldHistories, double waterCurtailmentRate)
        {
            _logger.Write($"AgroHydrology: start the processing of season preparation", Severity.Level2);

            HarvestableAlfalfa = 0;
            HarvestableBarley = 0;
            HarvestableWheat = 0;

            IrrigSeason = 0;

            foreach (var field in _fields)
            {
                WaterInField[field] = 0;
            }

            // Documented to calculate WaterInAquiferChange at the end of season.
            WaterInAquiferPrior = WaterInAquifer;
            _logger.Write($"WaterInAquifer = {WaterInAquiferPrior} at the beginning of the season", Severity.Level3);
            _logger.Write($"WaterCurtailmentRate = {waterCurtailmentRate}", Severity.Level3);

            WaterUsageMax = _parameters.WaterCurtailmentBase * (1 - waterCurtailmentRate / 100) / AcInToFt3;
            _logger.Write($"WaterUsageMax = {WaterUsageMax} for current season", Severity.Level3);

            // The maximum amount of water that can evaporate from field when accounting for plant type and field size.
            foreach (var fieldHistory in fieldHistories.Where(fh => fh.Plant != Plant.Nothing))
            {
                EvapTransFromFieldSeasonMax[fieldHistory.Field] =
                    _cropEvapTrans.Select(et => et.GetEvapTrans(fieldHistory.Plant)).Sum();
                _logger.Write(
                    $"EvapTransFromFieldSeasonMax = {EvapTransFromFieldSeasonMax[fieldHistory.Field]} for field {fieldHistory.Field.FieldNumber}", Severity.Level3);
            }

            _logger.Write($"AgroHydrology: finish the processing of season preparation", Severity.Level2);
        }

        private const double AcInToFt3 = 3.630;

        private const double RainToSnow = 10;

        #endregion
    }
}