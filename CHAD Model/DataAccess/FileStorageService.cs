using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using CHAD.Model;
using CHAD.Model.AgroHydrologyModule;
using CHAD.Model.ClimateModule;
using CHAD.Model.Infrastructure;
using CHAD.Model.RVACModule;
using CHAD.Model.SimulationResults;
using CHADSOSIEL.Configuration;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Field = CHAD.Model.AgroHydrologyModule.Field;
using Parameters = CHAD.Model.Parameters;

namespace CHAD.DataAccess
{
    public partial class FileStorageService : IStorageService
    {
        #region Static Fields and Constants

        private const string ClimateInput = "InputClimate.xlsx";
        private const string ClimateOutput = "Climate.xlsx";
        private const string ConfigurationsFolder = "Configurations";
        private const string CropEvapTransInput = "InputPlantEvapTrans.xlsx";
        private const string DecisionMakingOutput = "OutputDecisionMaking.xlsx";
        private const string DefaultConfigurationFolder = "Templates";
        private const string DroughtLevelInput = "InputDrought.xlsx";
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

        private List<string> GetAvailableSosielConfigurations(string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            if (!directoryInfo.Exists)
                throw new DirectoryNotFoundException(path);

            var result = new List<string>();

            foreach (var enumerateFile in directoryInfo.EnumerateFiles())
            {
                if (enumerateFile.Name.StartsWith("configuration", StringComparison.OrdinalIgnoreCase) &&
                    enumerateFile.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    result.Add(enumerateFile.Name);
            }

            return result;
        }

        public Configuration GetDefaultConfiguration()
        {
            var configuration = new Configuration();

            var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultConfigurationFolder);

            return GetConfiguration(configuration, configurationPath);
        }

        public static string MakeConfigPathFolder(string configurationName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ConfigurationsFolder, configurationName);
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
            SaveDroughtLevelConfiguration(Path.Combine(configurationPath, DroughtLevelInput), configuration);
            SaveCropEvapTransConfiguration(Path.Combine(configurationPath, CropEvapTransInput), configuration);
            SaveInputFieldConfiguration(Path.Combine(configurationPath, FieldsInput), configuration);
            SaveMarketPrices(Path.Combine(configurationPath, MarketPricesInput), configuration);
            SaveSOSIELConfiguration(Path.Combine(configurationPath));
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
                configuration.CropEvapTransList.Add(new CropEvapTrans(Convert.ToInt32(row[0].ToString()),
                    ToDouble(row[1].ToString()), ToDouble(row[2].ToString()), ToDouble(row[3].ToString())));
        }

        private void FillDroughtLevelConfiguration(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.DroughtLevels.Clear();

            configuration.DroughtLevels.AddRange(table.Rows.Cast<DataRow>()
                .Select(e => new DroughtLevel(int.Parse(e[0].ToString()), ToDouble(e[1].ToString()))));
        }

        private void FillFieldsConfiguration(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.Fields.Clear();

            configuration.Fields.AddRange(table.Rows.Cast<DataRow>()
                .Select(e => new Field(int.Parse(e[0].ToString()), ToDouble(e[1].ToString()), ToDouble(e[2].ToString()))));
        }

        private void FillMarketPrices(string path, Configuration configuration)
        {
            var table = GetTable(path);

            configuration.MarketPrices.Clear();

            configuration.MarketPrices.AddRange(table
                .Rows.Cast<DataRow>()
                .Select(e => new MarketPrice(int.Parse(e[0].ToString()),
                    ToDouble(e[1].ToString()),
                    ToDouble(e[2].ToString()),
                    ToDouble(e[3].ToString()),
                    ToDouble(e[4].ToString())
                )));
        }

        private void FillParameters(string path, Configuration configuration)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var xmlSerializer = new XmlSerializer(typeof(Parameters));
                var parameters = (Parameters)xmlSerializer.Deserialize(fileStream);
                configuration.Parameters = parameters;
            }
        }

        private void FillSOSIELConfiguration(string path, Configuration configuration)
        {
            configuration.SOSIELConfiguration = ConfigurationParser.ParseConfiguration(path);
            if (configuration.Name != null)
                configuration.SOSIELConfiguration.ConfigurationPath = MakeConfigPathFolder(configuration.Name);
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

            configuration.AvailableSosielConfigurations = GetAvailableSosielConfigurations(configurationPath);
            FillParameters(Path.Combine(configurationPath, Parameters), configuration);
            if (string.IsNullOrEmpty(configuration.Parameters.SosielConfiguration))
                configuration.Parameters.SosielConfiguration = configuration.AvailableSosielConfigurations.First();
            FillClimateConfiguration(Path.Combine(configurationPath, ClimateInput), configuration);
            FillCropEvapTransConfiguration(Path.Combine(configurationPath, CropEvapTransInput), configuration);
            FillFieldsConfiguration(Path.Combine(configurationPath, FieldsInput), configuration);
            FillMarketPrices(Path.Combine(configurationPath, MarketPricesInput), configuration);
            FillSOSIELConfiguration(Path.Combine(configurationPath, configuration.Parameters.SosielConfiguration), configuration);
            FillDroughtLevelConfiguration(Path.Combine(configurationPath, DroughtLevelInput), configuration);

            return configuration;
        }

        private DataTable GetTable(string fileName)
        {
            var dataTable = new DataTable();
            using (var spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                var sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                var relationshipId = sheets.First().Id.Value;
                var worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                var workSheet = worksheetPart.Worksheet;
                var sheetData = workSheet.GetFirstChild<SheetData>();
                var rows = sheetData.Descendants<Row>();

                foreach (var openXmlElement in rows.ElementAt(0))
                {
                    var cell = (Cell)openXmlElement;
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
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
                    row = new Row { RowIndex = rowIndex };
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
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
                    foreach (var dailyClimate in seasonResult.ClimateResult.DailyClimate)
                    {
                        // Add a row to the cell table.
                        row = new Row { RowIndex = rowIndex };
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
                sheetData.Append(row);

                var days = configuration.CropEvapTransList.Any()
                    ? configuration.CropEvapTransList.Max(item => item.Day)
                    : 0;

                var newCell = row.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue("Day");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("Alfalfa");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 2);
                newCell.CellValue = new CellValue("Barley");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 3);
                newCell.CellValue = new CellValue("Wheat");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var cropEvapTrans in configuration.CropEvapTransList)
                {
                    row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(cropEvapTrans.Day.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(cropEvapTrans.AlfalfaValue.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue = new CellValue(cropEvapTrans.BarleyValue.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 3);
                    newCell.CellValue = new CellValue(cropEvapTrans.WheatValue.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    rowIndex++;
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
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
                    row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(seasonResult.Number.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue =
                        new CellValue(
                            seasonResult.WaterCurtailmentRate.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue =
                        new CellValue(seasonResult.RVACResult.ProfitTotal.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 3);
                    newCell.CellValue =
                        new CellValue(seasonResult.RVACResult.ProfitAlfalfa.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 4);
                    newCell.CellValue =
                        new CellValue(seasonResult.RVACResult.ProfitBarley.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 5);
                    newCell.CellValue =
                        new CellValue(seasonResult.RVACResult.ProfitCRP.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 6);
                    newCell.CellValue =
                        new CellValue(seasonResult.RVACResult.ProfitWheat.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    rowIndex++;
                }

                // Save the new worksheet.
                spreadsheetDocument.Save();
            }
        }

        private void SaveDroughtLevelConfiguration(string path, Configuration configuration)
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
                sheetData.Append(row);

                var newCell = row.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue("Season");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("Drought");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var field in configuration.DroughtLevels)
                {
                    // Add a row to the cell table.
                    row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(field.SeasonNumber.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(field.Value.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    rowIndex++;
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
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
                    foreach (var hydrology in seasonResult.DailyHydrology)
                    {
                        // Add a row to the cell table.
                        row = new Row { RowIndex = rowIndex };
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

                        rowIndex++;
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
                sheetData.Append(row);

                var newCell = row.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue("FieldNum");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("FieldSize");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue("WaterInField");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                var rowIndex = 2u;

                foreach (var field in configuration.Fields)
                {
                    // Add a row to the cell table.
                    row = new Row { RowIndex = rowIndex };
                    sheetData.Append(row);

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(field.FieldNumber.ToString());
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue(field.FieldSize.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    newCell = row.InsertAt(new Cell(), 2);
                    newCell.CellValue = new CellValue(field.InitialWaterVolume.ToString(CultureInfo.InvariantCulture));
                    newCell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    rowIndex++;
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
                { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);

                // Get the sheetData cell table.
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Add a row to the cell table.
                var row = new Row { RowIndex = 1 };
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
                    row = new Row { RowIndex = rowIndex };
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

                    rowIndex++;
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

        private void SaveSOSIELConfiguration(string path)
        {
            var assembly = Assembly.GetAssembly(typeof(FileStorageService));

            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                var nameBuilder = new StringBuilder(resourceName);
                nameBuilder.Replace("CHAD.DataAccess.Templates.", string.Empty);
                var name = nameBuilder.ToString();

                if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    var outputPath = Path.Combine(path, name);

                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    using (Stream outputStream = new FileStream(outputPath, FileMode.Create))
                    {
                        stream.CopyTo(outputStream);
                    }
                }
            }

            assembly = Assembly.GetAssembly(typeof(ConfigurationModel));

            var otherResourcesName = new[] { "CHADSOSIEL.general_probability.csv" };

            foreach (var anotherResourceName in otherResourcesName)
            {
                var fileName = anotherResourceName.Replace("CHADSOSIEL.", string.Empty);
                using (var stream = assembly.GetManifestResourceStream(anotherResourceName))
                using (Stream outputStream =
                    new FileStream(Path.Combine(Path.Combine(path, fileName)), FileMode.Create))
                {
                    stream.CopyTo(outputStream);
                }
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