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
        private readonly Dictionary<Field, double> IrrigOfFieldSeason;
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
            IrrigOfFieldSeason = new Dictionary<Field, double>(fieldsCount);
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
                IrrigOfFieldSeason[field] = 0;
                PercFromField[field] = 0;
                PrecipOnField[field] = 0;
                WaterInput[field] = 0;
                WaterInField[field] = 0;
                WaterInFieldMax[field] = Math.Round(_parameters.WaterStoreCap * field.FieldSize, 2);
            }
        }

        #endregion

        #region Public Interface

        public List<DailyHydrology> DailyHydrology { get; }

        public double HarvestableAlfalfa { get; private set; }

        public double HarvestableBarley { get; private set; }

        public double HarvestableWheat { get; private set; }

        public void ProcessDay(int dayNumber, DailyClimate dailyClimate, List<FieldHistory> fieldHistories)
        {
            foreach (var fieldHistory in fieldHistories)
            {
                var field = fieldHistory.Field;
                _logger.Write($"Process field number {field.FieldNumber}");
                _logger.Write($"{fieldHistory.Plant} is planted on the field");

                PrecipOnField[field] = dailyClimate.Precipitation;

                var MeltingRate = Math.Max(0, (dailyClimate.Temperature - _parameters.MeltingPoint) / 100);

                if (dailyClimate.Temperature > _parameters.MeltingPoint)
                {
                    _logger.Write("Temperature > MeltingPoint.");
                    PrecipOnField[field] = dailyClimate.Precipitation +
                                           MeltingRate * (WaterInSnowpack / RainToSnow);
                    WaterInSnowpack = WaterInSnowpack - MeltingRate * WaterInSnowpack;
                }
                else
                {
                    _logger.Write("Temperature <= MeltingPoint.");
                    PrecipOnField[field] = 0;
                    WaterInSnowpack = WaterInSnowpack + PrecipOnField[field] * RainToSnow;
                }

                PrecipOnField[field] = PrecipOnField[field] *
                                       (fieldHistory.Field.FieldSize / fieldHistories.Sum(f => f.Field.FieldSize));
                _logger.Write($"PrecipOnField = {PrecipOnField[field]}");
                _logger.Write($"WaterInSnowpack = {WaterInSnowpack}");

                if (fieldHistory.Plant == Plant.Nothing)
                    IrrigNeed[field] = 0;
                else
                    IrrigNeed[field] =
                        Math.Max(0, _cropEvapTrans.First(et => et.Day == dayNumber).GetEvapTrans(fieldHistory.Plant) *
                        fieldHistory.Field.FieldSize -
                        PrecipOnField[field]);

                _logger.Write($"IrrigNeed = {IrrigNeed[field]}");

                WaterUsageRemain = Math.Max(0, WaterUsageMax - IrrigSeason);
                _logger.Write($"WaterUsageRemain = {WaterUsageRemain}");

                IrrigOfField[field] = Math.Min(Math.Min(IrrigNeed[field], WaterUsageRemain), WaterInAquifer);
                _logger.Write($"IrrigOfField = {IrrigOfField[field]}");

                IrrigOfFieldSeason[field] = IrrigOfFieldSeason[field] + IrrigOfField[field];
                _logger.Write($"IrrigOfFieldSeason = {IrrigOfFieldSeason[field]}");

                IrrigSeason = IrrigOfFieldSeason.Sum(i => i.Value);
                _logger.Write($"IrrigSeason = {IrrigSeason}");

                DirectRunoff[field] = (PrecipOnField[field] + IrrigOfField[field]) *
                                      (double) Math.Pow((double) WaterInField[field] / (double) WaterInFieldMax[field],
                                          (double) _parameters.Beta);
                _logger.Write($"DirectRunoff = {DirectRunoff}");

                WaterInput[field] = PrecipOnField[field] + IrrigOfField[field] - DirectRunoff[field];
                _logger.Write($"WaterInput = {WaterInput}");

                WaterInField[field] = WaterInField[field] + AcInToFt3 * WaterInput[field];
                _logger.Write($"WaterInField + AcInToFt3 * WaterInput = {WaterInField}");

                if (fieldHistory.Plant == Plant.Nothing)
                    EvapTransFromField[field] = 0;
                else
                    EvapTransFromField[field] =
                        Math.Min(
                            _cropEvapTrans.First(et => et.Day == dayNumber).GetEvapTrans(fieldHistory.Plant) *
                            fieldHistory.Field.FieldSize, WaterInField[field] / AcInToFt3);
                _logger.Write($"EvapTransFromField = {EvapTransFromField[field]}");

                WaterInField[field] = WaterInField[field] - EvapTransFromField[field];
                _logger.Write($"WaterInField - EvapTransFromField = {WaterInField}");

                WaterInAquifer = WaterInAquifer - AcInToFt3 * IrrigOfField.Sum(i => i.Value);
                _logger.Write($"WaterInAquifer = {WaterInAquifer}");

                PercFromField[field] = Math.Min(_parameters.PercFromFieldFrac * WaterInField[field],
                    _parameters.WaterInAquiferMax - WaterInAquifer);
                _logger.Write($"PercFromField = {PercFromField[field]}");

                WaterInField[field] = WaterInField[field] - PercFromField[field];
                _logger.Write($"WaterInField - PercFromField = {WaterInField}");
            }

            WaterInAquifer = WaterInAquifer + PercFromField.Sum(i => i.Value);
            _logger.Write($"WaterInAquifer = {WaterInAquifer}");

            LeakAquifer = _parameters.LeakAquiferFrac * WaterInAquifer;
            _logger.Write($"LeakAquifer = {LeakAquifer}");

            WaterInAquifer = WaterInAquifer - LeakAquifer;
            _logger.Write($"WaterInAquifer = {WaterInAquifer}");

            WaterInAquiferChange = Math.Abs(WaterInAquifer - WaterInAquiferPrior);
            _logger.Write($"WaterInAquiferChange = {WaterInAquiferChange}");

            foreach (var fieldHistory in fieldHistories)
                EvapTransFromFieldToDate[fieldHistory.Field] =
                    EvapTransFromFieldToDate[fieldHistory.Field] + EvapTransFromField[fieldHistory.Field];
            _logger.Write($"EvapTransFromFieldToDate + EvapTransFromField = {EvapTransFromFieldToDate}");

            DailyHydrology.Add(new DailyHydrology(dayNumber, WaterInAquifer, WaterInSnowpack));
        }

        public void ProcessSeasonEnd(List<FieldHistory> fieldHistories)
        {
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

            _logger.Write($"HarvestableAlfalfa = {HarvestableAlfalfa}");
            _logger.Write($"HarvestableBarley = {HarvestableBarley}");
            _logger.Write($"HarvestableWheat = {HarvestableWheat}");
        }

        public void ProcessSeasonStart(List<FieldHistory> fieldHistories, double waterCurtailmentRate)
        {
            HarvestableAlfalfa = 0;
            HarvestableBarley = 0;
            HarvestableWheat = 0;

            foreach (var field in _fields)
            {
                IrrigOfFieldSeason[field] = 0;
                WaterInField[field] = 0;
            }

            // Documented to calculate WaterInAquiferChange at the end of season.
            WaterInAquiferPrior = WaterInAquifer;
            _logger.Write($"WaterInAquifer = {WaterInAquiferPrior} at the beginning of the season");
            _logger.Write($"WaterCurtailmentRate = {waterCurtailmentRate}");

            WaterUsageMax = _parameters.WaterCurtailmentBase * (1 - waterCurtailmentRate / 100) / AcInToFt3;
            _logger.Write($"WaterUsageMax = {WaterUsageMax} for current season");

            // The maximum amount of water that can evaporate from field when accounting for plant type and field size.
            foreach (var fieldHistory in fieldHistories.Where(fh => fh.Plant != Plant.Nothing))
            {
                EvapTransFromFieldSeasonMax[fieldHistory.Field] =
                    _cropEvapTrans.Select(et => et.GetEvapTrans(fieldHistory.Plant)).Sum();
                _logger.Write(
                    $"EvapTransFromFieldSeasonMax = {EvapTransFromFieldSeasonMax[fieldHistory.Field]} for field {fieldHistory.Field.FieldNumber}");
            }
        }

        private const double AcInToFt3 = 3.630;

        private const double RainToSnow = 10;

        #endregion
    }
}