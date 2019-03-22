using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Model;
using Model.ClimateModule;
using Field = Model.Field;
using Parameters = Model.Parameters;

namespace DataAccess
{
    public class FileStorageService : IStorageService
    {
        #region Static Fields and Constants

        private const string ConfigurationsFolderName = "Configurations";
        private const string ClimateInput = "InputClimate.xlsx";
        private const string CropEvapTransInput = "InputCropEvapTrans.xlsx";
        private const string Fields = "InputFieldSize.xlsx";
        private const string Parameters = "Parameters.xml";

        private const string OutputFolderName = "Output";
        private const string ClimateSimulationName = "Climate.xlsx";
        private const string HydrologySimulationName = "Hydrology.xlsx";

        #endregion

        #region Public Interface

        public static string MakeSimulationPath(string configurationName, string simulationSession,
            int simulationNumber)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), OutputFolderName,
                $"{simulationSession} - {configurationName}",
                $"Simulation {simulationNumber:0000}");
        }

        public IEnumerable<Configuration> GetConfigurations()
        {
            var path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, ConfigurationsFolderName);

            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var configurations = new List<Configuration>();

            foreach (var directory in directoryInfo.GetDirectories())
                configurations.Add(new Configuration(directory.Name));

            return configurations;
        }

        public void SaveLogs(string path, SimpleLogger logger)
        {
            Directory.CreateDirectory(path);
            var stream = File.Create(path + "/info.txt");
            var sw = new StreamWriter(stream);

            foreach (var loggerEntry in logger.Entries) sw.WriteLine(loggerEntry.Text);

            sw.Close();
        }

        public void SaveClimate(string path, Climate climate)
        {
            var spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);

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
            
            foreach (var dailyClimate in climate.OrderBy(dc => dc.Day))
            {
                row = new Row {RowIndex = rowindex++};
                sheetData.Append(row);
                lastcell = null;
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue(dailyClimate.Day.ToString()),
                        DataType = new EnumValue<CellValues>(CellValues.Number)
                    }, lastcell);
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue(dailyClimate.Temperature.ToString(CultureInfo.InvariantCulture)),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, lastcell);
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue(dailyClimate.Precipitation.ToString(CultureInfo.InvariantCulture)),
                        DataType = new EnumValue<CellValues>(CellValues.String)
                    }, lastcell);
            }

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        public void SaveHydrology(string path, List<Hydrology> hydrology, IEnumerable<Field> fields)
        {
            var spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);

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
            for (var i = 0; i < fields.Count(); i++)
                lastcell = row.InsertAfter(
                    new Cell
                    {
                        CellValue = new CellValue("Field" + fields.ElementAt(i).FieldNum),
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

        public Configuration GetConfiguration(Configuration configuration, string configurationPath)
        {
            var directoryInfo = new DirectoryInfo(configurationPath);

            if (!directoryInfo.Exists)
                return configuration;

            FillParameters(Path.Combine(configurationPath, Parameters), configuration);
            FillClimateConfiguration(Path.Combine(configurationPath, ClimateInput), configuration);
            FillCropEvapTransConfiguration(Path.Combine(configurationPath, CropEvapTransInput), configuration);
            FillFieldsConfiguration(Path.Combine(configurationPath, Fields), configuration);

            return configuration;
        }

        public Configuration GetConfiguration(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(configuration.Name))
                throw new ArgumentException(nameof(configuration.Name));

            var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigurationsFolderName,
                configuration.Name);

            return GetConfiguration(configuration, configurationPath);
        }

        public Configuration GetConfiguration(string name, string path)
        {
            var configuration = new Configuration(name);

            try
            {
                GetConfiguration(configuration, path);
            }
            catch (Exception)
            {
                // ignored
            }

            return configuration;
        }

        public void SaveConfiguration(Configuration configuration, bool rewrite)
        {
            var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigurationsFolderName,
                configuration.Name);

            var directoryInfo = new DirectoryInfo(configurationPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();
            else if (directoryInfo.Exists && rewrite == false)
                throw new ConfigurationWithSameNameExistsException();

            SaveParameters(Path.Combine(configurationPath, Parameters), configuration);
            SaveClimateConfiguration(Path.Combine(configurationPath, ClimateInput), configuration);
            SaveCropEvapTransConfiguration(Path.Combine(configurationPath, CropEvapTransInput), configuration);
            SaveInputFieldConfiguration(Path.Combine(configurationPath, Fields), configuration);
        }

        public void SaveSimulationResult(SimulationResult simulationResult, SimulationResultPart simulationResultPart)
        {
            var simulationResultPath = MakeSimulationPath(simulationResult.Configuration.Name,
                simulationResult.SimulationSession, simulationResult.SimulationNumber);

            var directoryInfo = new DirectoryInfo(simulationResultPath);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            if (simulationResultPart.HasFlag(SimulationResultPart.Climate))
                SaveClimate(Path.Combine(simulationResultPath, ClimateSimulationName),
                    simulationResult.Climate);
            if (simulationResultPart.HasFlag(SimulationResultPart.Hydrology))
                SaveHydrology(Path.Combine(simulationResultPath, HydrologySimulationName),
                    simulationResult.AgroHydrology.Hydrology, simulationResult.Configuration.Fields);
        }

        #endregion

        #region All other members

        private void FillParameters(string path, Configuration configuration)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var xmlSerializer = new XmlSerializer(typeof(Parameters));
                var parameters = (Parameters) xmlSerializer.Deserialize(fileStream);
                configuration.Parameters = parameters;
            }
        }

        private void SaveParameters(string path, Configuration configuration)
        {
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(Parameters));
                xmlSerializer.Serialize(fileStream, configuration.Parameters);
            }
        }

        private void SaveClimateConfiguration(string path, Configuration configuration)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook.
                var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                var sheet = new Sheet
                    {Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet"};
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row {RowIndex = 1};
                sheetData.Append(row);

                var newCell = row.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue("Day");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("TempMean");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 2);
                newCell.CellValue = new CellValue("TempSD");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 3);
                newCell.CellValue = new CellValue("PrecipMean");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 4);
                newCell.CellValue = new CellValue("PrecipSD");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var climate in configuration.ClimateForecast)
                {
                    // Add a row to the cell table.
                    row = new Row {RowIndex = rowIndex};
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(climate.Day.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(climate.TempMean.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue = new CellValue(climate.TempSD.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 3);
                    newCell.CellValue = new CellValue(climate.PrecipMean.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 4);
                    newCell.CellValue = new CellValue(climate.PrecipSD.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    rowIndex++;
                }

                // Save the new worksheet.
                spreadsheetDocument.Save();
            }
        }

        private void SaveCropEvapTransConfiguration(string path, Configuration configuration)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook.
                var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                var sheet = new Sheet
                    {Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet"};
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row {RowIndex = 1};
                sheetData.Append(row);

                var days = configuration.CropEvapTransList.Any()
                    ? configuration.CropEvapTransList.Max(item => item.Day)
                    : 0;

                var plantGroups = configuration.CropEvapTransList.GroupBy(plant => plant.CropType)
                    .OrderByDescending(plantGroup => plantGroup.Key).ToList();

                var newCell = row.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue("Day");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var columnIndex = 1;

                foreach (var plantGroup in plantGroups)
                {
                    newCell = row.InsertAt(new Cell(), columnIndex);
                    newCell.CellValue = new CellValue(plantGroup.First().CropName);
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                }

                for (var day = 1u; day <= days; day++)
                {
                    // Add a row to the cell table.
                    row = new Row {RowIndex = day + 1};
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(day.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    foreach (var plantGroup in plantGroups)
                    {
                        newCell = row.InsertAt(new Cell(), columnIndex);
                        newCell.CellValue = new CellValue(configuration.CropEvapTransList
                            .First(item => item.Day == day && item.CropType == plantGroup.Key).Quantity
                            .ToString(CultureInfo.InvariantCulture));
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    }
                }

                // Save the new worksheet.
                spreadsheetDocument.Save();
            }
        }

        private void SaveInputFieldConfiguration(string path, Configuration configuration)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook.
                var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                var sheet = new Sheet
                    {Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet"};
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row {RowIndex = 1};
                sheetData.Append(row);

                var newCell = row.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue("Day");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("FieldSize");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var field in configuration.Fields)
                {
                    // Add a row to the cell table.
                    row = new Row {RowIndex = rowIndex};
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(field.FieldNum.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(field.FieldSize.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                }

                // Save the new worksheet.
                spreadsheetDocument.Save();
            }
        }

        private static void FillClimateConfiguration(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.ClimateForecast.Clear();

            configuration.ClimateForecast.AddRange(table.Rows
                .Cast<DataRow>().Select(e => new ClimateForecast
                {
                    Day = Convert.ToInt32(e[0].ToString()),
                    TempMean = ToDouble(e[1].ToString()),
                    TempSD = ToDouble(e[2].ToString()),
                    PrecipMean = ToDouble(e[3].ToString()),
                    PrecipSD = ToDouble(e[4].ToString()),
                    //TempMeanRandom = Gaussian(ToDouble(e[1].ToString()), ToDouble(e[2].ToString())),
                    //PrecipMeanRandom = Gaussian(ToDouble(e[3].ToString()), ToDouble(e[4].ToString()))
                }));
        }

        private static void FillCropEvapTransConfiguration(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.CropEvapTransList.Clear();

            foreach (DataRow row in table.Rows)
                for (var i = 1; i < table.Columns.Count; i++)
                    configuration.CropEvapTransList.Add(new InputCropEvapTrans
                    {
                        Day = Convert.ToInt32(row[0].ToString()),
                        CropType = i,
                        CropName = table.Columns[i].ColumnName,
                        Quantity = (decimal) ToDouble(row[i].ToString())
                    });
        }

        private static void FillFieldsConfiguration(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.Fields.Clear();

            configuration.Fields.AddRange(table.Rows.Cast<DataRow>().Select(e => new Field
                {FieldNum = int.Parse(e[0].ToString()), FieldSize = ToDecimal(e[1].ToString())}));
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

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            var stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                return stringTablePart.SharedStringTable.ChildElements[Convert.ToInt32(value)].InnerText;
            return value;
        }

        private static double ToDouble(string input)
        {
            var separator = input.Contains(".") ? '.' : ',';
            var newSeparator = !input.Contains(".") ? '.' : ',';
            if (double.TryParse(input, out var result))
                return result;
            return Convert.ToDouble(input.Replace(separator, newSeparator));
        }

        private static decimal ToDecimal(string input)
        {
            var separator = input.Contains(".") ? '.' : ',';
            var newSeparator = !input.Contains(".") ? '.' : ',';
            if (decimal.TryParse(input, out var result))
                return result;
            return Convert.ToDecimal(input.Replace(separator, newSeparator));
        }

        

        #endregion
    }
}