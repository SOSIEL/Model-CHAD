using System;
using System.Collections.Generic;
using System.Linq;
using Model.ClimateModule;

namespace Model.Modules
{
    public class AgroHydrology
    {
        #region Fields

        private readonly List<InputCropEvapTrans> _cropEvapTrans;
        private readonly List<Field> _fields;
        private readonly ILogger _logger;
        private readonly Parameters _parameters;

        private readonly int[] CropInField;
        private readonly decimal[] DirectRunoff;
        private readonly decimal[] EvapTransFromField;
        private readonly decimal[] EvapTransFromFieldToDate;
        private readonly decimal[] IrrigOfField;
        private readonly decimal[] IrrigOfFieldSeason;
        private readonly decimal[] PercFromField;
        private readonly decimal[] PrecipOnField;
        private readonly decimal[] WaterInField;
        private readonly decimal[] WaterInFieldMax;
        private readonly decimal[] WaterInput;

        private decimal WaterInAquifer;

        #endregion

        #region Constructors

        public AgroHydrology(ILogger logger, Parameters parameters, List<ClimateForecast> climateList, List<Field> fields,
            List<InputCropEvapTrans> cropEvapTranses)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ClimateList = climateList ?? throw new ArgumentNullException(nameof(climateList));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            _cropEvapTrans = cropEvapTranses ?? throw new ArgumentNullException(nameof(InputCropEvapTrans));

            DirectRunoff = new decimal[_fields.Count];
            EvapTransFromField = new decimal[_fields.Count];
            EvapTransFromFieldToDate = new decimal[_fields.Count];
            IrrigOfField = new decimal[_fields.Count];
            IrrigOfFieldSeason = new decimal[_fields.Count];
            PercFromField = new decimal[_fields.Count];
            PrecipOnField = new decimal[_fields.Count];
            WaterInField = new decimal[_fields.Count];
            WaterInFieldMax = new decimal[_fields.Count];
            WaterInput = new decimal[_fields.Count];

            var random = new Random();
            CropInField = new int[_fields.Count];
            for (var i = 0; i < CropInField.Length; i++)
                CropInField[i] = random.Next(1, cropEvapTranses.GroupBy(crop => crop.CropType).Count());

            for (var i = 0; i < _fields.Count; i++)
                WaterInFieldMax[i] = Math.Round(_parameters.WaterStorCap * _fields.ElementAt(i).FieldSize, 2);

            WaterInAquifer = _parameters.WaterInAquifer;
            Hydrology = new List<Hydrology>(_fields.Count);
        }

        #endregion

        #region Public Interface

        public IEnumerable<ClimateForecast> ClimateList { get; }

        public List<Hydrology> Hydrology { get; }

        public void ProcessDay(int i)
        {
            _logger.Write(string.Format(
                "\nDay[{0}]\n__________________________________________________________________________\n", i));

            var Temp = ClimateList.FirstOrDefault(e => e.Day == i).TempMeanRandom;
            var Precip = ClimateList.FirstOrDefault(e => e.Day == i).PrecipMeanRandom;

            _logger.Write(string.Format(
                "Temp(Day[{0}])=Random.Normal(InputClimate.TempMean,InputClimate.TempSD,Day)={1}", i, Temp));
            _logger.Write(
                string.Format(
                    "Precip(Day[{0}])=Random.Normal(InputClimate.PrecipMean,InputClimate.PrecipSD,Day)={1}\n", i,
                    Precip));

            _logger.Write(Environment.NewLine);

            for (var fld = 0; fld < _fields.Count; fld++)
            {
                PrecipOnField[fld] =
                    Math.Round(
                        Precip * (_fields.ElementAt(fld).FieldSize / _fields.Sum(e => e.FieldSize)),
                        2);
                _logger.Write(
                    string.Format(
                        "PrecipOnField(FieldNum[{0}])=Precip*(InputFieldSize.FieldSize(FieldNum[{0}])/sum(InputFieldSize.FieldSize)={1}/({2}/{3})={4}",
                        fld + 1, Precip, _fields.ElementAt(fld).FieldSize,
                        _fields.Sum(e => e.FieldSize), PrecipOnField[fld]));
            }

            _logger.Write(Environment.NewLine);
            for (var fld = 0; fld < _fields.Count; fld++)
            {
                DirectRunoff[fld] =
                    Math.Round(
                        (PrecipOnField[fld] + IrrigOfField[fld]) * (decimal) Math.Pow(
                            decimal.ToDouble(WaterInField[fld] / WaterInFieldMax[fld]),
                            decimal.ToDouble(_parameters.Beta)), 2);
                _logger.Write(
                    string.Format(
                        "DirectRunoff(FieldNum[{0}])=(PrecipOnField(FieldNum[{0}])+IrrigOfField(FieldNum[{0}]))*((WaterInField(FieldNum[{0}])/WaterInFieldMax(FieldNum[{0}]))^Beta)=({1}+{2})*(({3}/{4})^{5}) = {6}",
                        fld + 1, PrecipOnField[fld], IrrigOfField[fld],
                        WaterInField[fld], WaterInFieldMax[fld], _parameters.Beta, DirectRunoff[fld]));
            }

            _logger.Write(Environment.NewLine);
            for (var fld = 0; fld < _fields.Count; fld++)
            {
                WaterInput[fld] = PrecipOnField[fld] + IrrigOfField[fld] - DirectRunoff[fld];
                _logger.Write(string.Format(
                    "WaterInput(FieldNum[{0}])=PrecipOnField(FieldNum[{0}])+IrrigOfField(FieldNum[{0}])-DirectRunoff(FieldNum[{0}])={1}+{2}-{3}={4}",
                    fld + 1, PrecipOnField[fld], IrrigOfField[fld], DirectRunoff[fld],
                    PrecipOnField[fld] + IrrigOfField[fld] - DirectRunoff[fld]));
            }

            _logger.Write(Environment.NewLine);
            for (var fld = 0; fld < _fields.Count; fld++)
            {
                _logger.Write(string.Format(
                    "WaterInField(FieldNum[{0}])=WaterInField(FieldNum[{0}])+WaterInput(FieldNum[{0}])={1}+{2}={3}",
                    fld + 1, WaterInField[fld], WaterInput[fld], WaterInField[fld] + WaterInput[fld]));
                WaterInField[fld] = WaterInField[fld] + WaterInput[fld];
            }

            _logger.Write(Environment.NewLine);
            for (var fld = 0; fld < _fields.Count; fld++)
            {
                EvapTransFromField[fld] =
                    Math.Round(
                        Math.Min(
                            _cropEvapTrans.FirstOrDefault(e => e.Day == i && e.CropType == CropInField[fld])
                                .Quantity * _fields.ElementAt(fld).FieldSize, WaterInField[fld]), 2);
                _logger.Write(string.Format(
                    "EvapTransFromField(FieldNum[{0}])=min(InputCropEvapTrans(CropInField(FieldNum[{0}],Day)*InputFieldSize.FieldSize(FieldNum[{0}])),WaterInField(FieldNum[{0}]))=min({1}*{2},{3})=min({4},{5})={6}",
                    fld + 1,
                    _cropEvapTrans.FirstOrDefault(e => e.Day == i && e.CropType == CropInField[fld]).Quantity,
                    _fields.ElementAt(fld).FieldSize, WaterInField[fld],
                    _cropEvapTrans.FirstOrDefault(e => e.Day == i && e.CropType == CropInField[fld])
                        .Quantity * _fields.ElementAt(fld).FieldSize, WaterInField[fld],
                    EvapTransFromField[fld]));
            }

            _logger.Write(Environment.NewLine);
            _logger.Write(
                string.Format("WaterInAquifer=WaterInAquifer-sum(IrrigOfField)={0}-{1}={2}", WaterInAquifer,
                    IrrigOfField.Sum(e => e), WaterInAquifer + IrrigOfField.Sum(e => e)));
            WaterInAquifer = WaterInAquifer + IrrigOfField.Sum(e => e);

            _logger.Write(Environment.NewLine);
            for (var fld = 0; fld < _fields.Count; fld++)
            {
                PercFromField[fld] =
                    Math.Round(
                        Math.Min(
                            _parameters.PercFromFieldFrac *
                            (WaterInField[fld] - EvapTransFromField[fld]),
                            _parameters.WaterInAquiferMax - WaterInAquifer), 2);
                _logger.Write(string.Format(
                    "PercFromField(FieldNum[{0}])=min(PercFromFieldFrac*(WaterInField(FieldNum[{0}])-EvapTransFromField(FieldNum[{0}])),(WaterInAquiferMax-WaterInAquifer))=min({1}*({2}-{3}),{4}-{5})=min({6},{7})={8}",
                    fld + 1, _parameters.PercFromFieldFrac, WaterInField[fld],
                    EvapTransFromField[fld],
                    _parameters.WaterInAquiferMax, WaterInAquifer,
                    _parameters.PercFromFieldFrac * (WaterInField[fld] - EvapTransFromField[fld]),
                    _parameters.WaterInAquiferMax - WaterInAquifer, PercFromField[fld]));
            }

            _logger.Write(Environment.NewLine);
            for (var fld = 0; fld < _fields.Count; fld++)
            {
                _logger.Write(string.Format(
                    "WaterInField(FieldNum[{0}])=WaterInField(FieldNum[{0}])-PercFromField(FieldNum[{0}])={1}-{2}={3}",
                    fld + 1, WaterInField[fld], PercFromField[fld], WaterInField[fld] - PercFromField[fld]));
                WaterInField[fld] = WaterInField[fld] - PercFromField[fld];
            }

            _logger.Write(Environment.NewLine);
            _logger.Write(string.Format("WaterInAquifer=WaterInAquifer+sum(PercFromField)={0}+{1}={2}",
                WaterInAquifer,
                PercFromField.Sum(e => e), WaterInAquifer + PercFromField.Sum(e => e)));
            WaterInAquifer = WaterInAquifer + PercFromField.Sum(e => e);

            var LeakAquifer = Math.Round(_parameters.LeakAquiferFrac * WaterInAquifer, 2);
            _logger.Write(string.Format("LeakAquifer=LeakAquiferFrac*WaterInAquifer={0}*{1}={2}",
                _parameters.LeakAquiferFrac, WaterInAquifer, LeakAquifer));

            _logger.Write(string.Format("WaterInAquifer=WaterInAquifer-LeakAquifer={0}-{1}={2}\n\n", WaterInAquifer,
                LeakAquifer, WaterInAquifer - LeakAquifer));
            WaterInAquifer = WaterInAquifer - LeakAquifer;

            for (var fld = 0; fld < _fields.Count; fld++)
                Hydrology.Add(new Hydrology
                    {Day = i, Field = fld, WaterInAquifer = WaterInAquifer, WaterInField = WaterInField[fld]});
        }

        #endregion
    }
}