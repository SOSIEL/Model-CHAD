using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp10
{
    class Program
    {
        #region GetExcel
        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Convert.ToInt32(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        private static double ToDouble(String input)
        {
            char separator = input.Contains(".") ? '.' : ',';
            char newSeparator = !input.Contains(".") ? '.' : ',';
            double result;
            if (Double.TryParse(input, out result))
                return result;
            return Convert.ToDouble(input.Replace(separator, newSeparator));
        }

        public static DataTable GetTable(string fileName)
        {
            DataTable dataTable = new DataTable();
            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    dataTable.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                }

                foreach (Row row in rows)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        dataRow[i] = GetCellValue(spreadSheetDocument, row.Descendants<Cell>().ElementAt(i));
                    }

                    dataTable.Rows.Add(dataRow);
                }

            }
            dataTable.Rows.RemoveAt(0);

            return dataTable;
        }
        #endregion

        public static decimal Gaussian(Random random, double mean, double stddev)
        {
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();
            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return (decimal)Math.Round(y1 * stddev + mean, 2);
        }
        public static Random random = new Random();
        static void Main(string[] args)
        {
            IEnumerable<InputClimate> inputClimate = GetTable(ConfigurationManager.AppSettings["inputfilepath"] + "/InputClimate.xlsx").Rows.Cast<DataRow>().Select(e => new InputClimate
            {
                t = Convert.ToInt32(e[0].ToString()),
                TempMean = ToDouble(e[1].ToString()), // e[1] as double?,
                TempSD = ToDouble(e[2].ToString()),
                PrecipMean = ToDouble(e[3].ToString()),
                PrecipSD = ToDouble(e[4].ToString()),
                TempMeanRandom = (decimal)Gaussian(random, ToDouble(e[1].ToString()), ToDouble(e[2].ToString())),
                PrecipMeanRandom = (decimal)Gaussian(random, ToDouble(e[3].ToString()), ToDouble(e[4].ToString()))
            });
            List<InputCropEvapTrans> inputCropEvapTrans = new List<InputCropEvapTrans>();
            DataTable InputCropEvapTransTable = GetTable(ConfigurationManager.AppSettings["inputfilepath"] + "/InputCropEvapTrans.xlsx");
            foreach (DataRow row in InputCropEvapTransTable.Rows)
            {
                for (int i = 1; i < InputCropEvapTransTable.Columns.Count; i++)
                {
                    inputCropEvapTrans.Add(new InputCropEvapTrans()
                    {
                        t = Convert.ToInt32(row[0].ToString()),
                        CropType = i,
                        CropName = InputCropEvapTransTable.Columns[i].ColumnName,
                        Quantity = (decimal)ToDouble(row[i].ToString())
                    });
                }
            }

            IEnumerable<InputFieldSize> inputFieldSize;
            inputFieldSize = GetTable(ConfigurationManager.AppSettings["inputfilepath"] + "/InputFieldSize.xlsx").Rows.Cast<DataRow>().Select(e => new InputFieldSize
            {
                FieldNum = Configuration.ToDecimal(e[0].ToString()),
                FieldSize = Configuration.ToDecimal(e[1].ToString())
            });

            List<Hydrology> hydrology = new List<Hydrology>();


            decimal[] DirectRunoff = new decimal[inputFieldSize.Count()];
            int[] CropInField = new int[inputFieldSize.Count()];
            for (int i = 0; i < CropInField.Length; i++)
                CropInField[i] = random.Next(1, InputCropEvapTransTable.Columns.Count);

            decimal[] EvapTransFromField = new decimal[inputFieldSize.Count()];
            decimal[] IrrigOfField = new decimal[inputFieldSize.Count()];
            decimal[] PercFromField = new decimal[inputFieldSize.Count()];
            decimal[] PrecipOnField = new decimal[inputFieldSize.Count()];
            decimal[] WaterInField = new decimal[inputFieldSize.Count()];
            decimal[] WaterInFieldMax = new decimal[inputFieldSize.Count()];
            decimal[] WaterInput = new decimal[inputFieldSize.Count()];

            decimal WaterInAquifer = Configuration.WaterInAquifer;
            decimal LeakAquifer = 0;

            DateTime date = DateTime.Now;
            String path = ConfigurationManager.AppSettings["filepath"];
            path = path + "/" + date.ToString("dd_MM_yyyy_HH_mm_ss_fff");
            Directory.CreateDirectory(path);
            FileStream stream = File.Create(path + "/info.txt");
            StreamWriter sw = new StreamWriter(stream);
            try
            {
                for (int i = inputClimate.Min(e => e.t); i <= inputClimate.Max(e => e.t); i++)
                {
                    sw.WriteLine(String.Format("Day[{0}]\n__________________________________________________________________________\n", i));
                    decimal Temp = inputClimate.FirstOrDefault(e => e.t == i).TempMeanRandom;
                    decimal Precip = inputClimate.FirstOrDefault(e => e.t == i).PrecipMeanRandom;
                    sw.WriteLine(String.Format("Temp(Day[{0}])=Random.Normal(InputClimate.TempMean,InputClimate.TempSD,Day)={1}", i, Temp));
                    sw.WriteLine(String.Format("Precip(Day[{0}])=Random.Normal(InputClimate.PrecipMean,InputClimate.PrecipSD,Day)={1}\n", i, Precip));

                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                        WaterInFieldMax[fld] = Math.Round(Configuration.WaterStorCap * inputFieldSize.ElementAt(fld).FieldSize, 2);

                    sw.WriteLine("");

                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                    {
                        PrecipOnField[fld] = Math.Round(Precip * (inputFieldSize.ElementAt(fld).FieldSize / inputFieldSize.Sum(e => e.FieldSize)), 2);
                        sw.WriteLine(String.Format("PrecipOnField(FieldNum[{0}])=Precip*(InputFieldSize.FieldSize(FieldNum[{0}])/sum(InputFieldSize.FieldSize)={1}/({2}/{3})={4}", fld + 1, Precip, inputFieldSize.ElementAt(fld).FieldSize, inputFieldSize.Sum(e => e.FieldSize), PrecipOnField[fld]));
                    }
                    sw.WriteLine("");
                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                    {
                        DirectRunoff[fld] = Math.Round((PrecipOnField[fld] + IrrigOfField[fld]) * (decimal)Math.Pow(Decimal.ToDouble(WaterInField[fld] / WaterInFieldMax[fld]), Decimal.ToDouble(Configuration.Beta)), 2);
                        sw.WriteLine(String.Format("DirectRunoff(FieldNum[{0}])=(PrecipOnField(FieldNum[{0}])+IrrigOfField(FieldNum[{0}]))*((WaterInField(FieldNum[{0}])/WaterInFieldMax(FieldNum[{0}]))^Beta)=({1}+{2})*(({3}/{4})^{5}) = {6}",
                            fld + 1, PrecipOnField[fld], IrrigOfField[fld], WaterInField[fld], WaterInFieldMax[fld], Configuration.Beta, DirectRunoff[fld]));
                    }
                    sw.WriteLine("");
                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                    {
                        WaterInput[fld] = PrecipOnField[fld] + IrrigOfField[fld] - DirectRunoff[fld];
                        sw.WriteLine(String.Format("WaterInput(FieldNum[{0}])=PrecipOnField(FieldNum[{0}])+IrrigOfField(FieldNum[{0}])-DirectRunoff(FieldNum[{0}])={1}+{2}-{3}={4}",
                          fld + 1, PrecipOnField[fld], IrrigOfField[fld], DirectRunoff[fld], PrecipOnField[fld] + IrrigOfField[fld] - DirectRunoff[fld]));
                    }
                    sw.WriteLine("");
                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                    {
                        sw.WriteLine(String.Format("WaterInField(FieldNum[{0}])=WaterInField(FieldNum[{0}])+WaterInput(FieldNum[{0}])={1}+{2}={3}", fld + 1, WaterInField[fld], WaterInput[fld], WaterInField[fld] + WaterInput[fld]));
                        WaterInField[fld] = WaterInField[fld] + WaterInput[fld];
                    }
                    sw.WriteLine("");
                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                    {
                        EvapTransFromField[fld] = Math.Round(Math.Min(inputCropEvapTrans.FirstOrDefault(e => e.t == i && e.CropType == CropInField[fld]).Quantity * inputFieldSize.ElementAt(fld).FieldSize, WaterInField[fld]), 2);
                        sw.WriteLine(String.Format("EvapTransFromField(FieldNum[{0}])=min(InputCropEvapTrans(CropInField(FieldNum[{0}],Day)*InputFieldSize.FieldSize(FieldNum[{0}])),WaterInField(FieldNum[{0}]))=min({1}*{2},{3})=min({4},{5})={6}",
                        fld + 1, inputCropEvapTrans.FirstOrDefault(e => e.t == i && e.CropType == CropInField[fld]).Quantity, inputFieldSize.ElementAt(fld).FieldSize, WaterInField[fld], inputCropEvapTrans.FirstOrDefault(e => e.t == i && e.CropType == CropInField[fld]).Quantity * inputFieldSize.ElementAt(fld).FieldSize, WaterInField[fld], EvapTransFromField[fld]));
                    }
                    sw.WriteLine("");
                    sw.WriteLine(String.Format("WaterInAquifer=WaterInAquifer-sum(IrrigOfField)={0}-{1}={2}", WaterInAquifer, IrrigOfField.Sum(e => e), WaterInAquifer + IrrigOfField.Sum(e => e)));
                    WaterInAquifer = WaterInAquifer + IrrigOfField.Sum(e => e);
                    sw.WriteLine("");
                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                    {
                        PercFromField[fld] = Math.Round(Math.Min(Configuration.PercFromFieldFrac * (WaterInField[fld] - EvapTransFromField[fld]), Configuration.WaterInAquiferMax - WaterInAquifer), 2);
                        sw.WriteLine(String.Format("PercFromField(FieldNum[{0}])=min(PercFromFieldFrac*(WaterInField(FieldNum[{0}])-EvapTransFromField(FieldNum[{0}])),(WaterInAquiferMax-WaterInAquifer))=min({1}*({2}-{3}),{4}-{5})=min({6},{7})={8}",
                        fld + 1, Configuration.PercFromFieldFrac, WaterInField[fld], EvapTransFromField[fld], Configuration.WaterInAquiferMax, WaterInAquifer, Configuration.PercFromFieldFrac * (WaterInField[fld] - EvapTransFromField[fld]), Configuration.WaterInAquiferMax - WaterInAquifer, PercFromField[fld]));
                    }
                    sw.WriteLine("");
                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                    {
                        sw.WriteLine(String.Format("WaterInField(FieldNum[{0}])=WaterInField(FieldNum[{0}])-PercFromField(FieldNum[{0}])={1}-{2}={3}", fld + 1, WaterInField[fld], PercFromField[fld], WaterInField[fld] - PercFromField[fld]));
                        WaterInField[fld] = WaterInField[fld] - PercFromField[fld];
                    }
                    sw.WriteLine("");
                    sw.WriteLine(String.Format("WaterInAquifer=WaterInAquifer+sum(PercFromField)={0}+{1}={2}", WaterInAquifer, PercFromField.Sum(e => e), WaterInAquifer + PercFromField.Sum(e => e)));
                    WaterInAquifer = WaterInAquifer + PercFromField.Sum(e => e);

                    LeakAquifer = Math.Round(Configuration.LeakAquiferFrac * WaterInAquifer, 2);
                    sw.WriteLine(String.Format("LeakAquifer=LeakAquiferFrac*WaterInAquifer={0}*{1}={2}", Configuration.LeakAquiferFrac, WaterInAquifer, LeakAquifer));

                    sw.WriteLine(String.Format("WaterInAquifer=WaterInAquifer-LeakAquifer={0}-{1}={2}\n\n", WaterInAquifer, LeakAquifer, WaterInAquifer - LeakAquifer));
                    WaterInAquifer = WaterInAquifer - LeakAquifer;

                    for (int fld = 0; fld < inputFieldSize.Count(); fld++)
                        hydrology.Add(new Hydrology() { Day = i, Field = fld, WaterInAquifer = WaterInAquifer, WaterInField = WaterInField[fld] });
                }
            }
            catch (Exception ex)
            {
                sw.Close();
                throw new Exception(ex.Message, ex);
            }

            

            sw.Close();
            WriteFileHydrology(inputFieldSize, hydrology, path);
            WriteFileClimate(inputClimate, path);
        }

        static void WriteFileClimate(IEnumerable<InputClimate> inputClimate, String path)
        {
            Directory.CreateDirectory(path);
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(path + "/Climate.xlsx", SpreadsheetDocumentType.Workbook);


            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            UInt32Value rowindex = 1;
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            Row row;
            row = new Row() { RowIndex = rowindex++ };

            sheetData.Append(row);

            Cell lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue("Day"), DataType = new EnumValue<CellValues>(CellValues.String) }, null);
            lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue("Temp"), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);
            lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue("Precip"), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);

            IEnumerable<InputClimate> climate = inputClimate.OrderBy(e => e.t);
            foreach (InputClimate c in climate)
            {
                row = new Row() { RowIndex = rowindex++ };
                sheetData.Append(row);
                lastcell = null;
                lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue(c.t.ToString()), DataType = new EnumValue<CellValues>(CellValues.Number) }, lastcell);
                lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue(c.TempMeanRandom.ToString()), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);
                lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue(c.PrecipMeanRandom.ToString()), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);
            }

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        static void WriteFileHydrology(IEnumerable<InputFieldSize> inputFieldSize, List<Hydrology> hydrology, String path)
        {
            Directory.CreateDirectory(path);
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(path + "/Hydrology.xlsx", SpreadsheetDocumentType.Workbook);


            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            UInt32Value rowindex = 1;
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            Row row;
            row = new Row() { RowIndex = rowindex++ };

            sheetData.Append(row);

            Cell lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue("Day"), DataType = new EnumValue<CellValues>(CellValues.String) }, null);
            lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue("Aquifer"), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);
            for (int i = 0; i < inputFieldSize.Count(); i++)
                lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue("Field" + inputFieldSize.ElementAt(i).FieldNum), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);

            IEnumerable<Hydrology> hydrologyOrder = hydrology.OrderBy(e => e.Field).OrderBy(e => e.Day);
            int day = 0;
            foreach (Hydrology h in hydrologyOrder)
            {
                if (day != h.Day)
                {
                    row = new Row() { RowIndex = rowindex++ };
                    sheetData.Append(row);
                    day = h.Day;
                    lastcell = null;
                    lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue(h.Day.ToString()), DataType = new EnumValue<CellValues>(CellValues.Number) }, lastcell);
                    lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue(h.WaterInAquifer.ToString()), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);
                }

                lastcell = row.InsertAfter(new Cell() { CellValue = new CellValue(h.WaterInField.ToString()), DataType = new EnumValue<CellValues>(CellValues.String) }, lastcell);


            }

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }
    }
}
