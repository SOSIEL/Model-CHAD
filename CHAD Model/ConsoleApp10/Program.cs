using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Model;

namespace ConsoleApp10
{
    public class Program
    {
        #region Public Interface

        public static void Main(string[] args)
        {
            var random = new Random();
            var hydrology = new List<Hydrology>();

            var configuration = DataAccess.GetConfiguration();
            var inputClimate = DataAccess.GetClimate(random).ToList();
            var inputCropEvapTrans = DataAccess.GetCropEvapTrans().ToList();
            var inputFieldSize = DataAccess.GetFieldSizes().ToList();

            var DirectRunoff = new decimal[inputFieldSize.Count];
            var CropInField = new int[inputFieldSize.Count];
            for (var i = 0; i < CropInField.Length; i++)
                CropInField[i] = random.Next(1, DataAccess.InputCropEvapTransColumnsCount);

            var EvapTransFromField = new decimal[inputFieldSize.Count];
            var IrrigOfField = new decimal[inputFieldSize.Count];
            var PercFromField = new decimal[inputFieldSize.Count];
            var PrecipOnField = new decimal[inputFieldSize.Count];
            var WaterInField = new decimal[inputFieldSize.Count];
            var WaterInFieldMax = new decimal[inputFieldSize.Count];
            var WaterInput = new decimal[inputFieldSize.Count];

            var WaterInAquifer = configuration.Parameters.WaterInAquifer;
            decimal LeakAquifer;

            var date = DateTime.Now;
            var path = ConfigurationManager.AppSettings["filepath"];
            path = path + "/" + date.ToString("dd_MM_yyyy_HH_mm_ss_fff");
            Directory.CreateDirectory(path);
            var stream = File.Create(path + "/info.txt");
            var sw = new StreamWriter(stream);
            try
            {
                for (var i = inputClimate.Min(e => e.t); i <= inputClimate.Max(e => e.t); i++)
                {
                    sw.WriteLine("Day[{0}]\n__________________________________________________________________________\n", i);
                    var Temp = inputClimate.FirstOrDefault(e => e.t == i).TempMeanRandom;
                    var Precip = inputClimate.FirstOrDefault(e => e.t == i).PrecipMeanRandom;
                    sw.WriteLine("Temp(Day[{0}])=Random.Normal(InputClimate.TempMean,InputClimate.TempSD,Day)={1}", i,
                        Temp);
                    sw.WriteLine(
                        "Precip(Day[{0}])=Random.Normal(InputClimate.PrecipMean,InputClimate.PrecipSD,Day)={1}\n", i,
                        Precip);

                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                        WaterInFieldMax[fld] =
                            Math.Round(configuration.Parameters.WaterStorCap * inputFieldSize.ElementAt(fld).FieldSize,
                                2);

                    sw.WriteLine("");

                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                    {
                        PrecipOnField[fld] =
                            Math.Round(
                                Precip *
                                (inputFieldSize.ElementAt(fld).FieldSize / inputFieldSize.Sum(e => e.FieldSize)), 2);
                        sw.WriteLine(
                            "PrecipOnField(FieldNum[{0}])=Precip*(InputFieldSize.FieldSize(FieldNum[{0}])/sum(InputFieldSize.FieldSize)={1}/({2}/{3})={4}",
                            fld + 1, Precip, inputFieldSize.ElementAt(fld).FieldSize,
                            inputFieldSize.Sum(e => e.FieldSize), PrecipOnField[fld]);
                    }

                    sw.WriteLine("");
                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                    {
                        DirectRunoff[fld] =
                            Math.Round(
                                (PrecipOnField[fld] + IrrigOfField[fld]) * (decimal) Math.Pow(
                                    decimal.ToDouble(WaterInField[fld] / WaterInFieldMax[fld]),
                                    decimal.ToDouble(configuration.Parameters.Beta)), 2);
                        sw.WriteLine(
                            "DirectRunoff(FieldNum[{0}])=(PrecipOnField(FieldNum[{0}])+IrrigOfField(FieldNum[{0}]))*((WaterInField(FieldNum[{0}])/WaterInFieldMax(FieldNum[{0}]))^Beta)=({1}+{2})*(({3}/{4})^{5}) = {6}",
                            fld + 1, PrecipOnField[fld], IrrigOfField[fld], WaterInField[fld], WaterInFieldMax[fld],
                            configuration.Parameters.Beta, DirectRunoff[fld]);
                    }

                    sw.WriteLine("");
                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                    {
                        WaterInput[fld] = PrecipOnField[fld] + IrrigOfField[fld] - DirectRunoff[fld];
                        sw.WriteLine(
                            "WaterInput(FieldNum[{0}])=PrecipOnField(FieldNum[{0}])+IrrigOfField(FieldNum[{0}])-DirectRunoff(FieldNum[{0}])={1}+{2}-{3}={4}",
                            fld + 1, PrecipOnField[fld], IrrigOfField[fld], DirectRunoff[fld],
                            PrecipOnField[fld] + IrrigOfField[fld] - DirectRunoff[fld]);
                    }

                    sw.WriteLine("");
                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                    {
                        sw.WriteLine(
                            "WaterInField(FieldNum[{0}])=WaterInField(FieldNum[{0}])+WaterInput(FieldNum[{0}])={1}+{2}={3}",
                            fld + 1, WaterInField[fld], WaterInput[fld], WaterInField[fld] + WaterInput[fld]);
                        WaterInField[fld] = WaterInField[fld] + WaterInput[fld];
                    }

                    sw.WriteLine("");
                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                    {
                        EvapTransFromField[fld] =
                            Math.Round(
                                Math.Min(
                                    inputCropEvapTrans.FirstOrDefault(e => e.t == i && e.CropType == CropInField[fld])
                                        .Quantity * inputFieldSize.ElementAt(fld).FieldSize, WaterInField[fld]), 2);
                        sw.WriteLine(
                            "EvapTransFromField(FieldNum[{0}])=min(InputCropEvapTrans(CropInField(FieldNum[{0}],Day)*InputFieldSize.FieldSize(FieldNum[{0}])),WaterInField(FieldNum[{0}]))=min({1}*{2},{3})=min({4},{5})={6}",
                            fld + 1,
                            inputCropEvapTrans.FirstOrDefault(e => e.t == i && e.CropType == CropInField[fld]).Quantity,
                            inputFieldSize.ElementAt(fld).FieldSize, WaterInField[fld],
                            inputCropEvapTrans.FirstOrDefault(e => e.t == i && e.CropType == CropInField[fld])
                                .Quantity * inputFieldSize.ElementAt(fld).FieldSize, WaterInField[fld],
                            EvapTransFromField[fld]);
                    }

                    sw.WriteLine("");
                    sw.WriteLine("WaterInAquifer=WaterInAquifer-sum(IrrigOfField)={0}-{1}={2}", WaterInAquifer,
                        IrrigOfField.Sum(e => e), WaterInAquifer + IrrigOfField.Sum(e => e));
                    WaterInAquifer = WaterInAquifer + IrrigOfField.Sum(e => e);
                    sw.WriteLine("");
                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                    {
                        PercFromField[fld] =
                            Math.Round(
                                Math.Min(
                                    configuration.Parameters.PercFromFieldFrac *
                                    (WaterInField[fld] - EvapTransFromField[fld]),
                                    configuration.Parameters.WaterInAquiferMax - WaterInAquifer), 2);
                        sw.WriteLine(
                            "PercFromField(FieldNum[{0}])=min(PercFromFieldFrac*(WaterInField(FieldNum[{0}])-EvapTransFromField(FieldNum[{0}])),(WaterInAquiferMax-WaterInAquifer))=min({1}*({2}-{3}),{4}-{5})=min({6},{7})={8}",
                            fld + 1, configuration.Parameters.PercFromFieldFrac, WaterInField[fld],
                            EvapTransFromField[fld],
                            configuration.Parameters.WaterInAquiferMax, WaterInAquifer,
                            configuration.Parameters.PercFromFieldFrac * (WaterInField[fld] - EvapTransFromField[fld]),
                            configuration.Parameters.WaterInAquiferMax - WaterInAquifer, PercFromField[fld]);
                    }

                    sw.WriteLine("");
                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                    {
                        sw.WriteLine(
                            "WaterInField(FieldNum[{0}])=WaterInField(FieldNum[{0}])-PercFromField(FieldNum[{0}])={1}-{2}={3}",
                            fld + 1, WaterInField[fld], PercFromField[fld], WaterInField[fld] - PercFromField[fld]);
                        WaterInField[fld] = WaterInField[fld] - PercFromField[fld];
                    }

                    sw.WriteLine("");
                    sw.WriteLine("WaterInAquifer=WaterInAquifer+sum(PercFromField)={0}+{1}={2}", WaterInAquifer,
                        PercFromField.Sum(e => e), WaterInAquifer + PercFromField.Sum(e => e));
                    WaterInAquifer = WaterInAquifer + PercFromField.Sum(e => e);

                    LeakAquifer = Math.Round(configuration.Parameters.LeakAquiferFrac * WaterInAquifer, 2);
                    sw.WriteLine("LeakAquifer=LeakAquiferFrac*WaterInAquifer={0}*{1}={2}",
                        configuration.Parameters.LeakAquiferFrac, WaterInAquifer, LeakAquifer);

                    sw.WriteLine("WaterInAquifer=WaterInAquifer-LeakAquifer={0}-{1}={2}\n\n", WaterInAquifer,
                        LeakAquifer, WaterInAquifer - LeakAquifer);
                    WaterInAquifer = WaterInAquifer - LeakAquifer;

                    for (var fld = 0; fld < inputFieldSize.Count; fld++)
                        hydrology.Add(new Hydrology
                            {Day = i, Field = fld, WaterInAquifer = WaterInAquifer, WaterInField = WaterInField[fld]});
                }
            }
            catch (Exception ex)
            {
                sw.Close();
                throw new Exception(ex.Message, ex);
            }


            sw.Close();
            DataAccess.WriteFileHydrology(inputFieldSize, hydrology, path);
            DataAccess.WriteFileClimate(inputClimate, path);
        }

        #endregion
    }
}