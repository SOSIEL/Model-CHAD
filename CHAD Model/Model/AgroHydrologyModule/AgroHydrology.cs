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
        private readonly List<FieldHistory> _fieldHistories;
        private readonly ILogger _logger;
        private readonly Parameters _parameters;

        private readonly Dictionary<FieldHistory, decimal> DirectRunoff;
        private readonly Dictionary<FieldHistory, decimal> EvapTransFromField;

        private readonly Dictionary<FieldHistory, decimal> EvapTransFromFieldSeasonMax;
        private readonly Dictionary<FieldHistory, decimal> EvapTransFromFieldToDate;
        private readonly Dictionary<FieldHistory, decimal> IrrigNeed;
        private readonly Dictionary<FieldHistory, decimal> IrrigOfField;
        private readonly Dictionary<FieldHistory, decimal> IrrigOfFieldSeason;
        private readonly Dictionary<FieldHistory, decimal> PercFromField;
        private readonly Dictionary<FieldHistory, decimal> PrecipOnField;
        private readonly Dictionary<FieldHistory, decimal> WaterInField;
        private readonly Dictionary<FieldHistory, decimal> WaterInFieldMax;
        private readonly Dictionary<FieldHistory, decimal> WaterInput;
        private decimal IrrigSeason;
        private decimal LeakAquifer;
        private decimal WaterInAquifer;
        private decimal WaterInAquiferChange;
        private decimal WaterInAquiferGap;

        private decimal WaterInAquiferPrior;
        private decimal WaterInSnowpack;
        private decimal WaterUsageMax;
        private decimal WaterUsageRemain;

        #endregion

        #region Constructors

        public AgroHydrology(ILogger logger, Parameters parameters, List<FieldHistory> fieldHistories,
            List<InputCropEvapTrans> cropEvapTrans)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _fieldHistories = fieldHistories ?? throw new ArgumentNullException(nameof(fieldHistories));
            _cropEvapTrans = cropEvapTrans ?? throw new ArgumentNullException(nameof(InputCropEvapTrans));

            DailyHydrology = new List<DailyHydrology>();

            WaterInAquifer = _parameters.WaterInAquifer;
            WaterInSnowpack = _parameters.WaterInSnowpack;

            var fieldsCount = _fieldHistories.Count;

            EvapTransFromField = new Dictionary<FieldHistory, decimal>(fieldsCount);
            EvapTransFromFieldToDate = new Dictionary<FieldHistory, decimal>(fieldsCount);
            EvapTransFromFieldSeasonMax = new Dictionary<FieldHistory, decimal>(fieldsCount);
            DirectRunoff = new Dictionary<FieldHistory, decimal>(fieldsCount);
            IrrigNeed = new Dictionary<FieldHistory, decimal>(fieldsCount);
            IrrigOfField = new Dictionary<FieldHistory, decimal>(fieldsCount);
            IrrigOfFieldSeason = new Dictionary<FieldHistory, decimal>(fieldsCount);
            PercFromField = new Dictionary<FieldHistory, decimal>(fieldsCount);
            PrecipOnField = new Dictionary<FieldHistory, decimal>(fieldsCount);
            WaterInput = new Dictionary<FieldHistory, decimal>(fieldsCount);
            WaterInField = new Dictionary<FieldHistory, decimal>(fieldsCount);
            WaterInFieldMax = new Dictionary<FieldHistory, decimal>(fieldsCount);

            foreach (var fieldHistory in _fieldHistories)
            {
                EvapTransFromField[fieldHistory] = 0;
                EvapTransFromFieldToDate[fieldHistory] = 0;
                EvapTransFromFieldSeasonMax[fieldHistory] = 0;
                DirectRunoff[fieldHistory] = 0;
                IrrigNeed[fieldHistory] = 0;
                IrrigOfField[fieldHistory] = 0;
                IrrigOfFieldSeason[fieldHistory] = 0;
                PercFromField[fieldHistory] = 0;
                PrecipOnField[fieldHistory] = 0;
                WaterInput[fieldHistory] = 0;
                WaterInField[fieldHistory] = 0;
                WaterInFieldMax[fieldHistory] = Math.Round(_parameters.WaterStoreCap * fieldHistory.Field.FieldSize, 2);
            }
        }

        #endregion

        #region Public Interface

        public List<DailyHydrology> DailyHydrology { get; }

        public decimal HarvestableAlfalfa { get; private set; }
        public decimal HarvestableBarley { get; private set; }
        public decimal HarvestableWheat { get; private set; }

        public void ProcessDay(int dayNumber, DailyClimate dailyClimate)
        {
            foreach (var fieldHistory in _fieldHistories)
            {
                var precipitation = dailyClimate.Precipitation + _parameters.MeltingRate * _parameters.WaterInSnowpack;

                if (dailyClimate.Temperature > _parameters.MeltingPoint)
                    precipitation = dailyClimate.Precipitation + _parameters.MeltingRate * WaterInSnowpack;
                else
                    WaterInSnowpack = WaterInSnowpack + precipitation;

                PrecipOnField[fieldHistory] = precipitation * (fieldHistory.Field.FieldSize / _fieldHistories.Sum(f => f.Field.FieldSize));

                if (fieldHistory.Plant == Plant.Nothing)
                    IrrigNeed[fieldHistory] = 0;
                else
                    IrrigNeed[fieldHistory] =
                        _cropEvapTrans.First(et => et.Day == dayNumber && et.Plant == fieldHistory.Plant).Quantity *
                        fieldHistory.Field.FieldSize;

                WaterUsageRemain = Math.Max(0, WaterUsageMax - IrrigSeason);

                IrrigOfField[fieldHistory] = Math.Min(Math.Min(IrrigNeed[fieldHistory], WaterUsageRemain), WaterInAquifer);

                IrrigOfFieldSeason[fieldHistory] = IrrigOfFieldSeason[fieldHistory] + IrrigOfField[fieldHistory];

                IrrigSeason = IrrigOfFieldSeason.Sum(i => i.Value);

                DirectRunoff[fieldHistory] = (PrecipOnField[fieldHistory] + IrrigOfField[fieldHistory]) *
                                      (decimal) Math.Pow((double) WaterInField[fieldHistory] / (double) WaterInFieldMax[fieldHistory],
                                          (double) _parameters.Beta);

                WaterInput[fieldHistory] = PrecipOnField[fieldHistory] + IrrigOfField[fieldHistory] - DirectRunoff[fieldHistory];

                WaterInField[fieldHistory] = WaterInField[fieldHistory] + WaterInput[fieldHistory];

                if (fieldHistory.Plant == Plant.Nothing)
                    EvapTransFromField[fieldHistory] = 0;
                else
                    EvapTransFromField[fieldHistory] =
                        Math.Min(
                            _cropEvapTrans.First(et => et.Day == dayNumber && et.Plant == fieldHistory.Plant).Quantity *
                            fieldHistory.Field.FieldSize, WaterInField[fieldHistory]);

                WaterInField[fieldHistory] = WaterInField[fieldHistory] - EvapTransFromField[fieldHistory];

                WaterInAquifer = WaterInAquifer - IrrigOfField.Sum(i => i.Value);

                PercFromField[fieldHistory] = Math.Min(_parameters.PercFromFieldFrac * WaterInField[fieldHistory],
                    _parameters.WaterInAquiferMax - WaterInAquifer);

                WaterInField[fieldHistory] = WaterInField[fieldHistory] - PercFromField[fieldHistory];
            }

            WaterInAquifer = WaterInAquifer + PercFromField.Sum(i => i.Value);

            LeakAquifer = _parameters.LeakAquiferFrac * WaterInAquifer;

            WaterInAquifer = WaterInAquifer - LeakAquifer;

            WaterInAquiferChange = Math.Abs(WaterInAquifer - WaterInAquiferPrior);

            WaterInAquiferGap = Math.Abs(WaterInAquifer - _parameters.SustainableLevelAquifer);

            foreach (var field in _fieldHistories)
                EvapTransFromFieldToDate[field] = EvapTransFromFieldToDate[field] + EvapTransFromField[field];

            DailyHydrology.Add(new DailyHydrology(dayNumber, WaterInAquifer, WaterInSnowpack));
        }

        public void ProcessSeasonEnd()
        {
            foreach (var field in _fieldHistories)
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

        public void ProcessSeasonStart(decimal waterCurtailmentRate)
        {
            WaterCurtailmentRate = waterCurtailmentRate;
            WaterInAquiferPrior = WaterInAquifer;

            WaterUsageMax = _parameters.WaterCurtailmentBase * (1 - _parameters.WaterCurtailmentRate / 100);

            foreach (var field in _fieldHistories)
                EvapTransFromFieldSeasonMax[field] =
                    _cropEvapTrans.Where(c => c.Plant == field.Plant).Sum(c => c.Quantity);
        }

        public decimal WaterCurtailmentRate { get; private set; }

        #endregion
    }
}