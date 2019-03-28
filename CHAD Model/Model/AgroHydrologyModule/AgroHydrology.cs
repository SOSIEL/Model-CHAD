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

        private readonly List<InputCropEvapTrans> _cropEvapTrans;
        private readonly List<Field> _fields;
        private readonly ILogger _logger;
        private readonly Parameters _parameters;

        private decimal WaterInAquiferPrior;
        private decimal WaterInAquiferChange;
        private decimal WaterInAquiferGap;
        private decimal WaterInAquifer;
        public decimal WaterCurtailmentRate { get; private set; }
        private decimal LeakAquifer;
        private decimal IrrigSeason;

        private readonly Dictionary<Field, decimal> EvapTransFromFieldSeasonMax;
        private readonly Dictionary<Field, decimal> EvapTransFromField;
        private readonly Dictionary<Field, decimal> EvapTransFromFieldToDate;

        private readonly Dictionary<Field, decimal> DirectRunoff;
        private readonly Dictionary<Field, decimal> IrrigNeed;
        private readonly Dictionary<Field, decimal> IrrigOfField;
        private readonly Dictionary<Field, decimal> IrrigOfFieldSeason;
        private readonly Dictionary<Field, decimal> PercFromField;
        private readonly Dictionary<Field, decimal> PrecipOnField;
        private readonly Dictionary<Field, decimal> WaterInField;
        private readonly Dictionary<Field, decimal> WaterInFieldMax;
        private readonly Dictionary<Field, decimal> WaterInput;
        private decimal WaterUsageMax;
        private decimal WaterUsageRemain;
        private decimal WaterInSnowpack;

        public decimal HarvestableAlfalfa { get; private set; }
        public decimal HarvestableBarley { get; private set; }
        public decimal HarvestableWheat { get; private set; }

        #endregion

        #region Constructors

        public AgroHydrology(ILogger logger, Parameters parameters, List<Field> fields,
            List<InputCropEvapTrans> cropEvapTrans)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            _cropEvapTrans = cropEvapTrans ?? throw new ArgumentNullException(nameof(InputCropEvapTrans));

            DailyHydrology = new List<DailyHydrology>();

            WaterInAquifer = _parameters.WaterInAquifer;
            WaterInSnowpack = _parameters.WaterInSnowpack;

            var fieldsCount = _fields.Count;

            EvapTransFromField = new Dictionary<Field, decimal>(fieldsCount);
            EvapTransFromFieldToDate = new Dictionary<Field, decimal>(fieldsCount);
            EvapTransFromFieldSeasonMax = new Dictionary<Field, decimal>(fieldsCount);
            DirectRunoff = new Dictionary<Field, decimal>(fieldsCount);
            IrrigNeed = new Dictionary<Field, decimal>(fieldsCount);
            IrrigOfField = new Dictionary<Field, decimal>(fieldsCount);
            IrrigOfFieldSeason = new Dictionary<Field, decimal>(fieldsCount);
            PercFromField = new Dictionary<Field, decimal>(fieldsCount);
            PrecipOnField = new Dictionary<Field, decimal>(fieldsCount);
            WaterInput = new Dictionary<Field, decimal>(fieldsCount);
            WaterInField = new Dictionary<Field, decimal>(fieldsCount);
            WaterInFieldMax = new Dictionary<Field, decimal>(fieldsCount);

            foreach (var field in _fields)
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

        public List<DailyHydrology> DailyHydrology { get; private set; }

        public void ProcessSeasonStart()
        {
            WaterInAquiferPrior = WaterInAquifer;

            WaterUsageMax = _parameters.WaterCurtailmentBase * (1 - _parameters.WaterCurtailmentRate / 100);

            foreach (var field in _fields)
                EvapTransFromFieldSeasonMax[field] = _cropEvapTrans.Where(c => c.Plant == field.Plant).Sum(c => c.Quantity);
        }

        public void ProcessDay(int dayNumber, DailyClimate dailyClimate)
        {
            foreach (var field in _fields)
            {
                var precipitation = dailyClimate.Precipitation + _parameters.MeltingRate * _parameters.WaterInSnowpack;

                if (dailyClimate.Temperature > _parameters.MeltingPoint)
                    precipitation = dailyClimate.Precipitation + _parameters.MeltingRate * WaterInSnowpack;
                else
                    WaterInSnowpack = WaterInSnowpack + precipitation;

                PrecipOnField[field] = precipitation * (field.FieldSize / _fields.Sum(f => f.FieldSize));

                IrrigNeed[field] =
                    _cropEvapTrans.First(et => et.Day == dayNumber && et.Plant == field.Plant).Quantity *
                    field.FieldSize;

                WaterUsageRemain = Math.Max(0, WaterUsageMax - IrrigSeason);

                IrrigOfField[field] = Math.Min(Math.Min(IrrigNeed[field], WaterUsageRemain), WaterInAquifer);

                IrrigOfFieldSeason[field] = IrrigOfFieldSeason[field] + IrrigOfField[field];

                IrrigSeason = IrrigOfFieldSeason.Sum(i => i.Value);

                DirectRunoff[field] = (PrecipOnField[field] + IrrigOfField[field]) *
                                      (decimal) Math.Pow((double) WaterInField[field] / (double) WaterInFieldMax[field],
                                          (double) _parameters.Beta);

                WaterInput[field] = PrecipOnField[field] + IrrigOfField[field] - DirectRunoff[field];

                WaterInField[field] = WaterInField[field] + WaterInput[field];

                EvapTransFromField[field] =
                    Math.Min(
                        _cropEvapTrans.First(et => et.Day == dayNumber && et.Plant == field.Plant).Quantity *
                        field.FieldSize, WaterInField[field]);

                WaterInField[field] = WaterInField[field] - EvapTransFromField[field];

                WaterInAquifer = WaterInAquifer - IrrigOfField.Sum(i => i.Value);

                PercFromField[field] = Math.Min(_parameters.PercFromFieldFrac * WaterInField[field],
                    _parameters.WaterInAquiferMax - WaterInAquifer);

                WaterInField[field] = WaterInField[field] - PercFromField[field];
            }

            WaterInAquifer = WaterInAquifer + PercFromField.Sum(i => i.Value);

            LeakAquifer = _parameters.LeakAquiferFrac * WaterInAquifer;

            WaterInAquifer = WaterInAquifer - LeakAquifer;

            WaterInAquiferChange = Math.Abs(WaterInAquifer - WaterInAquiferPrior);

            WaterInAquiferGap = Math.Abs(WaterInAquifer - _parameters.SustainableLevelAquifer);

            foreach (var field in _fields)
                EvapTransFromFieldToDate[field] = EvapTransFromFieldToDate[field] + EvapTransFromField[field];

            DailyHydrology.Add(new DailyHydrology(dayNumber, WaterInAquifer, WaterInSnowpack));
        }

        public void ProcessSeasonEnd()
        {
            foreach (var field in _fields)
            {
                if (field.Plant == Plant.Alfalfa)
                    HarvestableAlfalfa = HarvestableAlfalfa + EvapTransFromFieldToDate[field] /
                                         EvapTransFromFieldSeasonMax[field];

                if (field.Plant == Plant.Barley)
                    HarvestableBarley = HarvestableBarley + EvapTransFromFieldToDate[field] /
                                         EvapTransFromFieldSeasonMax[field];

                if (field.Plant == Plant.Wheat)
                    HarvestableWheat = HarvestableWheat + EvapTransFromFieldToDate[field] /
                                         EvapTransFromFieldSeasonMax[field];
            }
        }

        #endregion
    }
}