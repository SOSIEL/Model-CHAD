using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CHAD.Model;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.Infrastructure;
using CHAD.Model.RVACModule;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Field = CHAD.Model.AgroHydrologyModule.Field;
using Parameters = CHAD.Model.Parameters;

namespace CHAD.DataAccess
{
    public class FileStorageService : IStorageService
    {
        #region Static Fields and Constants

        private const string ClimateInput = "InputClimate.xlsx";
        private const string ClimateOutput = "Climate.xlsx";

        private const string ConfigurationsFolder = "Configurations";
        private const string CropEvapTransInput = "InputCropEvapTrans.xlsx";
        private const string DecisionMakingOutput = "Hydrology.xlsx";
        private const string FieldsInput = "InputFieldSize.xlsx";
        private const string HydrologyOutput = "Hydrology.xlsx";
        private const string MarketPricesInput = "InputFinancials.xlsx";

        private const string OutputFolder = "Output";
        private const string Parameters = "Parameters.xml";

        #endregion

        #region Public Interface

        public Configuration GetConfiguration(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(configuration.Name))
                throw new ArgumentException(nameof(configuration.Name));

            var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigurationsFolder,
                configuration.Name);

            return GetConfiguration(configuration, configurationPath);
        }

        public IEnumerable<Configuration> GetConfigurations()
        {
            var path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, ConfigurationsFolder);

            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var configurations = new List<Configuration>();

            foreach (var directory in directoryInfo.GetDirectories())
                configurations.Add(new Configuration(directory.Name));

            return configurations;
        }

        public static string MakeSimulationPath(string configurationName, string simulationSession,
            int simulationNumber)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), OutputFolder,
                $"{simulationSession} - {configurationName}",
                $"Simulation {simulationNumber:0000}");
        }

        public void SaveConfiguration(Configuration configuration, bool rewrite)
        {
            var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigurationsFolder,
                configuration.Name);

            var directoryInfo = new DirectoryInfo(configurationPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();
            else if (directoryInfo.Exists && rewrite == false)
                throw new ConfigurationWithSameNameExistsException();

            SaveParameters(Path.Combine(configurationPath, Parameters), configuration);
            SaveClimateConfiguration(Path.Combine(configurationPath, ClimateInput), configuration);
            SaveCropEvapTransConfiguration(Path.Combine(configurationPath, CropEvapTransInput), configuration);
            SaveInputFieldConfiguration(Path.Combine(configurationPath, FieldsInput), configuration);
            SaveMarketPrices(Path.Combine(configurationPath, MarketPricesInput), configuration);
        }

        public void SaveSimulationResult(SimulationResult simulationResult)
        {
            var simulationResultPath = MakeSimulationPath(simulationResult.Configuration.Name,
                simulationResult.SimulationSession, simulationResult.SimulationNumber);

            var directoryInfo = new DirectoryInfo(simulationResultPath);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            SaveClimateOutput(Path.Combine(simulationResultPath, ClimateOutput), simulationResult);
            SaveHydrologyOutput(Path.Combine(simulationResultPath, HydrologyOutput), simulationResult);
            SaveDecisionMaking(Path.Combine(simulationResultPath, DecisionMakingOutput), simulationResult);
        }

        #endregion

        #region All other members

        private void FillClimateConfiguration(string path, Configuration configuration)
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
                    PrecipSD = ToDouble(e[4].ToString())
                }));
        }

        private void FillCropEvapTransConfiguration(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.CropEvapTransList.Clear();

            foreach (DataRow row in table.Rows)
                for (var i = 1; i <= 3; i++)
                    configuration.CropEvapTransList.Add(new InputCropEvapTrans
                    {
                        Day = Convert.ToInt32(row[0].ToString()),
                        Plant = i == 1 ? Plant.Alfalfa : i == 2 ? Plant.Barley : Plant.Wheat,
                        Quantity = (decimal) ToDouble(row[i].ToString())
                    });
        }

        private void FillFieldsConfiguration(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.Fields.Clear();

            configuration.Fields.AddRange(table.Rows.Cast<DataRow>().Select(e => new Field
                (int.Parse(e[0].ToString()), ToDecimal(e[1].ToString()))));
        }

        private void FillMarketPrices(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.MarketPrices.Clear();

            configuration.MarketPrices.AddRange(table
                .Rows.Cast<DataRow>()
                .Select(e => new MarketPrice(int.Parse(e[0].ToString()),
                    ToDecimal(e[1].ToString()),
                    ToDecimal(e[2].ToString()),
                    ToDecimal(e[3].ToString()),
                    ToDecimal(e[4].ToString())
                )));
        }

        private void FillParameters(string path, Configuration configuration)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var xmlSerializer = new XmlSerializer(typeof(Parameters));
                var parameters = (Parameters) xmlSerializer.Deserialize(fileStream);
                configuration.Parameters = parameters;
            }
        }

        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            var stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                return stringTablePart.SharedStringTable.ChildElements[Convert.ToInt32(value)].InnerText;
            return value;
        }

        private Configuration GetConfiguration(Configuration configuration, string configurationPath)
        {
            var directoryInfo = new DirectoryInfo(configurationPath);

            if (!directoryInfo.Exists)
                return configuration;

            FillParameters(Path.Combine(configurationPath, Parameters), configuration);
            FillClimateConfiguration(Path.Combine(configurationPath, ClimateInput), configuration);
            FillCropEvapTransConfiguration(Path.Combine(configurationPath, CropEvapTransInput), configuration);
            FillFieldsConfiguration(Path.Combine(configurationPath, FieldsInput), configuration);
            FillMarketPrices(Path.Combine(configurationPath, MarketPricesInput), configuration);

            return configuration;
        }

        private DataTable GetTable(string fileName)
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

        private void SaveClimateOutput(string path, SimulationResult simulationResult)
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
                newCell.CellValue = new CellValue("Season");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("Day");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 2);
                newCell.CellValue = new CellValue("Temp");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 3);
                newCell.CellValue = new CellValue("Precip");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var seasonResult in simulationResult)
                foreach (var dailyClimate in seasonResult.Climate)
                {
                    // Add a row to the cell table.
                    row = new Row {RowIndex = rowIndex};
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(seasonResult.Number.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(dailyClimate.Day.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue =
                        new CellValue(dailyClimate.Temperature.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 3);
                    newCell.CellValue =
                        new CellValue(dailyClimate.Precipitation.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
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

                var plantGroups = configuration.CropEvapTransList.GroupBy(plant => plant.Plant)
                    .OrderByDescending(plantGroup => plantGroup.Key).ToList();

                var newCell = row.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue("Day");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var columnIndex = 1;

                foreach (var plantGroup in plantGroups)
                {
                    newCell = row.InsertAt(new Cell(), columnIndex);
                    newCell.CellValue = new CellValue(plantGroup.First().Plant.ToString());
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
                            .First(item => item.Day == day && item.Plant == plantGroup.Key).Quantity
                            .ToString(CultureInfo.InvariantCulture));
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    }
                }

                // Save the new worksheet.
                spreadsheetDocument.Save();
            }
        }

        private void SaveDecisionMaking(string path, SimulationResult simulationResult)
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
                newCell.CellValue = new CellValue("Season");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("Water\nCurtailmentRate");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 2);
                newCell.CellValue = new CellValue("ProfitTotal");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 3);
                newCell.CellValue = new CellValue("ProfitAlfalfa");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 4);
                newCell.CellValue = new CellValue("ProfitBarley");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 5);
                newCell.CellValue = new CellValue("ProfitCRP");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 6);
                newCell.CellValue = new CellValue("ProfitWheat");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var seasonResult in simulationResult)
                {
                    // Add a row to the cell table.
                    row = new Row {RowIndex = rowIndex};
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(seasonResult.Number.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(seasonResult.AgroHydrology.WaterCurtailmentRate.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue = new CellValue(seasonResult.RVAC.ProfitTotal.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 3);
                    newCell.CellValue = new CellValue(seasonResult.RVAC.ProfitAlfalfa.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 4);
                    newCell.CellValue = new CellValue(seasonResult.RVAC.ProfitBarley.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 5);
                    newCell.CellValue = new CellValue(seasonResult.RVAC.ProfitCRP.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 6);
                    newCell.CellValue = new CellValue(seasonResult.RVAC.ProfitWheat.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                // Save the new worksheet.
                spreadsheetDocument.Save();
            }
        }

        private void SaveHydrologyOutput(string path, SimulationResult simulationResult)
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
                newCell.CellValue = new CellValue("Season");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("Day");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 2);
                newCell.CellValue = new CellValue("Aquifer");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 3);
                newCell.CellValue = new CellValue("Snowpack");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var seasonResult in simulationResult)
                foreach (var hydrology in seasonResult.AgroHydrology.DailyHydrology)
                {
                    // Add a row to the cell table.
                    row = new Row {RowIndex = rowIndex};
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(seasonResult.Number.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(hydrology.Day.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue = new CellValue(hydrology.WaterInAquifer.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 3);
                    newCell.CellValue = new CellValue(hydrology.WaterInSnowpack.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
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

        private void SaveMarketPrices(string path, Configuration configuration)
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
                newCell.CellValue = new CellValue("Season");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("MarketPriceAlfalfa");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 2);
                newCell.CellValue = new CellValue("MarketPriceBarley");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 3);
                newCell.CellValue = new CellValue("MarketPriceWheat");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 4);
                newCell.CellValue = new CellValue("SubsidyCRP");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var marketPrice in configuration.MarketPrices)
                {
                    // Add a row to the cell table.
                    row = new Row {RowIndex = rowIndex};
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(marketPrice.SeasonNumber.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue =
                        new CellValue(marketPrice.MarketPriceAlfalfa.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue =
                        new CellValue(marketPrice.MarketPriceBarley.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 3);
                    newCell.CellValue =
                        new CellValue(marketPrice.MarketPriceWheat.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 4);
                    newCell.CellValue = new CellValue(marketPrice.SubsidyCRP.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                // Save the new worksheet.
                spreadsheetDocument.Save();
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

        private decimal ToDecimal(string input)
        {
            var separator = input.Contains(".") ? '.' : ',';
            var newSeparator = !input.Contains(".") ? '.' : ',';
            if (decimal.TryParse(input, out var result))
                return result;
            return Convert.ToDecimal(input.Replace(separator, newSeparator));
        }

        private double ToDouble(string input)
        {
            var separator = input.Contains(".") ? '.' : ',';
            var newSeparator = !input.Contains(".") ? '.' : ',';
            if (double.TryParse(input, out var result))
                return result;
            return Convert.ToDouble(input.Replace(separator, newSeparator));
        }

        #endregion
    }
}