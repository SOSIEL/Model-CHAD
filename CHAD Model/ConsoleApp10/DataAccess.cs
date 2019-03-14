using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Model;
using Configuration = Model.Configuration;

namespace ConsoleApp10
{
    public static class DataAccess
    {
        #region Static Fields and Constants

        private static int? _inputCropEvapTransColumnsCount;

        #endregion

        #region Public Interface

        public static int InputCropEvapTransColumnsCount
        {
            get
            {
                if (!_inputCropEvapTransColumnsCount.HasValue)
                    throw new NullReferenceException();

                return _inputCropEvapTransColumnsCount.Value;
            }
        }

        public static void WriteFileClimate(IEnumerable<InputClimate> inputClimate, string path)
        {
            Directory.CreateDirectory(path);
            var spreadsheetDocument =
                SpreadsheetDocument.Create(path + "/Climate.xlsx", SpreadsheetDocumentType.Workbook);


            var workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            var sheet = new Sheet
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            UInt32Value rowindex = 1;
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            Row row;
            row = new Row {RowIndex = rowindex++};

            sheetData.Append(row);

            var lastcell =
                row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue("Day"),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, null);
            lastcell = row.InsertAfter(
                new Cell {CellValue = new CellValue("Temp"), DataType = new EnumValue<CellValues>(CellValues.String)},
                lastcell);
            lastcell = row.InsertAfter(
                new Cell {CellValue = new CellValue("Precip"), DataType = new EnumValue<CellValues>(CellValues.String)},
                lastcell);

            IEnumerable<InputClimate> climate = inputClimate.OrderBy(e => e.t);
            foreach (var c in climate)
            {
                row = new Row {RowIndex = rowindex++};
                sheetData.Append(row);
                lastcell = null;
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue(c.t.ToString()),
                        DataType = new EnumValue<CellValues>(CellValues.Number)
                    }, lastcell);
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue(c.TempMeanRandom.ToString()),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, lastcell);
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue(c.PrecipMeanRandom.ToString()),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, lastcell);
            }

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        public static void WriteFileHydrology(IEnumerable<InputFieldSize> inputFieldSize, List<Hydrology> hydrology,
            string path)
        {
            Directory.CreateDirectory(path);
            var spreadsheetDocument =
                SpreadsheetDocument.Create(path + "/Hydrology.xlsx", SpreadsheetDocumentType.Workbook);


            var workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            var sheet = new Sheet
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            UInt32Value rowindex = 1;
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            Row row;
            row = new Row {RowIndex = rowindex++};

            sheetData.Append(row);

            var lastcell =
                row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue("Day"),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, null);
            lastcell = row.InsertAfter(
                new Cell
                {
                    CellValue = new CellValue("Aquifer"),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                }, lastcell);
            for (var i = 0; i < inputFieldSize.Count(); i++)
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue("Field" + inputFieldSize.ElementAt(i).FieldNum),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, lastcell);

            IEnumerable<Hydrology> hydrologyOrder = hydrology.OrderBy(e => e.Field).OrderBy(e => e.Day);
            var day = 0;
            foreach (var h in hydrologyOrder)
            {
                if (day != h.Day)
                {
                    row = new Row {RowIndex = rowindex++};
                    sheetData.Append(row);
                    day = h.Day;
                    lastcell = null;
                    lastcell = row.InsertAfter(
                        new Cell
                        {
                            CellValue = new CellValue(h.Day.ToString()),
                            DataType = new EnumValue<CellValues>(CellValues.Number)
                        }, lastcell);
                    lastcell = row.InsertAfter(
                        new Cell
                        {
                            CellValue = new CellValue(h.WaterInAquifer.ToString()),
                            DataType = new EnumValue<CellValues>(CellValues.String)
                        }, lastcell);
                }

                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue(h.WaterInField.ToString()),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, lastcell);
            }

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        public static Configuration GetConfiguration()
        {
            var configuration = new Configuration();

            FillParameters(configuration);
            FillClimate(configuration);
            FillCropEvapTrans(configuration);
            FillFieldSizes(configuration);


            return configuration;
        }

        #endregion

        #region All other members

        private static void FillClimate(Configuration configuration)
        {
            var table = GetTable(ConfigurationManager.AppSettings["inputfilepath"] + "/InputClimate.xlsx");

            configuration.ClimateList.AddRange(table.Rows
                .Cast<DataRow>().Select(e => new InputClimate
                {
                    t = Convert.ToInt32(e[0].ToString()),
                    TempMean = ToDouble(e[1].ToString()), // e[1] as double?,
                    TempSD = ToDouble(e[2].ToString()),
                    PrecipMean = ToDouble(e[3].ToString()),
                    PrecipSD = ToDouble(e[4].ToString()),
                    TempMeanRandom = Gaussian(ToDouble(e[1].ToString()), ToDouble(e[2].ToString())),
                    PrecipMeanRandom = Gaussian(ToDouble(e[3].ToString()), ToDouble(e[4].ToString()))
                }));
        }

        private static void FillCropEvapTrans(Configuration configuration)
        {
            var table = GetTable(ConfigurationManager.AppSettings["inputfilepath"] + "/InputCropEvapTrans.xlsx");

            foreach (DataRow row in table.Rows)
                for (var i = 1; i < table.Columns.Count; i++)
                    configuration.CropEvapTransList.Add(new InputCropEvapTrans
                    {
                        t = Convert.ToInt32(row[0].ToString()),
                        CropType = i,
                        CropName = table.Columns[i].ColumnName,
                        Quantity = (decimal) ToDouble(row[i].ToString())
                    });

            _inputCropEvapTransColumnsCount = table.Columns.Count;
        }

        private static void FillFieldSizes(Configuration configuration)
        {
            var table = GetTable(ConfigurationManager.AppSettings["inputfilepath"] + "/InputFieldSize.xlsx");

            configuration.FieldSizeList.AddRange(table.Rows.Cast<DataRow>().Select(e => new InputFieldSize
                {FieldNum = ToDecimal(e[0].ToString()), FieldSize = ToDecimal(e[1].ToString())}));
        }

        private static void FillParameters(Configuration configuration)
        {
            configuration.Parameters.WaterInAquifer = ToDecimal(ConfigurationManager.AppSettings["WaterInAquifer"]);
            configuration.Parameters.WaterInAquiferMax =
                ToDecimal(ConfigurationManager.AppSettings["WaterInAquiferMax"]);
            configuration.Parameters.Beta = ToDecimal(ConfigurationManager.AppSettings["Beta"]);
            configuration.Parameters.LeakAquiferFrac = ToDecimal(ConfigurationManager.AppSettings["LeakAquiferFrac"]);
            configuration.Parameters.PercFromFieldFrac =
                ToDecimal(ConfigurationManager.AppSettings["PercFromFieldFrac"]);
            configuration.Parameters.WaterStorCap = ToDecimal(ConfigurationManager.AppSettings["WaterStorCap"]);
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            var stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                return stringTablePart.SharedStringTable.ChildElements[Convert.ToInt32(value)].InnerText;
            return value;
        }

        private static DataTable GetTable(string fileName)
        {
            var dataTable = new DataTable();
            using (var spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                var sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                var relationshipId = sheets.First().Id.Value;
                var worksheetPart = (WorksheetPart) spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                var workSheet = worksheetPart.Worksheet;
                var sheetData = workSheet.GetFirstChild<SheetData>();
                var rows = sheetData.Descendants<Row>();

                foreach (var openXmlElement in rows.ElementAt(0))
                {
                    var cell = (Cell) openXmlElement;
                    dataTable.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                }

                foreach (var row in rows)
                {
                    var dataRow = dataTable.NewRow();
                    for (var i = 0; i < row.Descendants<Cell>().Count(); i++)
                        dataRow[i] = GetCellValue(spreadSheetDocument, row.Descendants<Cell>().ElementAt(i));

                    dataTable.Rows.Add(dataRow);
                }
            }

            dataTable.Rows.RemoveAt(0);

            return dataTable;
        }

        private static double ToDouble(string input)
        {
            var separator = input.Contains(".") ? '.' : ',';
            var newSeparator = !input.Contains(".") ? '.' : ',';
            double result;
            if (double.TryParse(input, out result))
                return result;
            return Convert.ToDouble(input.Replace(separator, newSeparator));
        }

        private static decimal ToDecimal(string input)
        {
            var separator = input.Contains(".") ? '.' : ',';
            var newSeparator = !input.Contains(".") ? '.' : ',';
            decimal result;
            if (decimal.TryParse(input, out result))
                return result;
            return Convert.ToDecimal(input.Replace(separator, newSeparator));
        }

        private static decimal Gaussian(double mean, double stddev)
        {
            var random = new Random();
            var x1 = 1 - random.NextDouble();
            var x2 = 1 - random.NextDouble();
            var y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return (decimal) Math.Round(y1 * stddev + mean, 2);
        }

        #endregion
    }
}