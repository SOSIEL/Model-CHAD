using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using CHAD.Model.Infrastructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CHAD.DataAccess
{
    public partial class FileStorageService
    {
        #region Static Fields and Constants

        private const int AfterDaysRecordCount = 1;

        private const string HydrologyDetailedOutput = "OutputHydrologyDetailed.xlsx";

        #endregion

        #region Fields

        private readonly List<string> _afterFieldsRecordNames = new List<string>
        {
            SimulationInfo.LeakAquifer,
            SimulationInfo.WaterInAquifer3,
            SimulationInfo.WaterInAquiferChange,
            SimulationInfo.HarvestableAlfalfa,
            SimulationInfo.HarvestableBarley,
            SimulationInfo.HarvestableWheat
        };

        private readonly List<string> _beforeFieldsRecordNames =
            new List<string> {SimulationInfo.Precip, SimulationInfo.SnowInSnowpack};

        private readonly List<string> _fieldRecordNames = new List<string>
        {
            SimulationInfo.Plant,
            SimulationInfo.PrecipOnField,
            SimulationInfo.IrrigNeed,
            SimulationInfo.IrrigOfField,
            SimulationInfo.IrrigSeason,
            SimulationInfo.DirectRunoff,
            SimulationInfo.WaterInput,
            SimulationInfo.WaterInField,
            SimulationInfo.EvapTransFromField,
            SimulationInfo.WaterInField2,
            SimulationInfo.WaterInAquifer,
            SimulationInfo.PercFromField,
            SimulationInfo.WaterInAquifer2,
            SimulationInfo.WaterInField3,
            SimulationInfo.EvapTransToDate
        };

        #endregion

        #region Public Interface

        public void SaveAgroHydrologyResults(SimulationInfo simulationInfo,
            IEnumerable<AgroHydrologyRecord> records)
        {
            var simulationResultPath = MakeSimulationPath(simulationInfo.ConfigurationName,
                simulationInfo.SimulationSession, simulationInfo.SimulationNumber);

            var directoryInfo = new DirectoryInfo(simulationResultPath);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var filePath = Path.Combine(simulationResultPath, HydrologyDetailedOutput);

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                CreateAgroHydrologyDetailedFile(filePath, simulationInfo);

            SaveAgroHydrologyResults(filePath, simulationInfo, records);
        }

        #endregion

        #region All other members

        private void CreateAgroHydrologyDetailedFile(string path, SimulationInfo simulationInfo)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                //var workbookStylesPart = spreadsheetDocument.AddNewPart<WorkbookStylesPart>();
                //workbookStylesPart.Stylesheet = CreateStylesheet();

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
                var row1 = new Row {RowIndex = 1};
                var row2 = new Row {RowIndex = 2};
                sheetData.Append(row1);
                sheetData.Append(row2);

                var newCell = row1.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue(string.Empty);
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row2.InsertAt(new Cell(), 0);
                newCell.CellValue = new CellValue(string.Empty);
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row1.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue(string.Empty);
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                newCell = row2.InsertAt(new Cell(), 1);
                newCell.CellValue = new CellValue(string.Empty);
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                for (var seasonNumber = 1; seasonNumber <= simulationInfo.SeasonCount; seasonNumber++)
                {
                    for (var dayNumber = 1; dayNumber <= simulationInfo.DaysCount; dayNumber++)
                    {
                        newCell = row1.InsertAt(new Cell(),
                            (seasonNumber - 1) * (simulationInfo.DaysCount + AfterDaysRecordCount) + dayNumber + 1);
                        newCell.CellValue = new CellValue($"Season {seasonNumber}");
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                        newCell = row2.InsertAt(new Cell(),
                            (seasonNumber - 1) * (simulationInfo.DaysCount + AfterDaysRecordCount) + dayNumber + 1);
                        newCell.CellValue = new CellValue($"Day {dayNumber}");
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    }

                    newCell = row1.InsertAt(new Cell(), seasonNumber * simulationInfo.DaysCount + seasonNumber + 1);
                    newCell.CellValue = new CellValue($"Season {seasonNumber}");
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    newCell = row2.InsertAt(new Cell(), seasonNumber * simulationInfo.DaysCount + seasonNumber + 1);
                    newCell.CellValue = new CellValue("Total");
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                }

                var rowIndex = 2u;

                for (var j = 1; j <= _beforeFieldsRecordNames.Count; j++)
                {
                    var row = new Row {RowIndex = ++rowIndex};

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(string.Empty);
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue($"{_beforeFieldsRecordNames[j - 1]}");
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    sheetData.Append(row);
                }

                for (var i = 1; i <= simulationInfo.FieldNames.Count; i++)
                for (var j = 1; j <= _fieldRecordNames.Count; j++)
                {
                    var row = new Row {RowIndex = ++rowIndex};

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue($"{simulationInfo.FieldNames[i - 1]}");
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue($"{_fieldRecordNames[j - 1]}");
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    sheetData.Append(row);
                }

                for (var j = 1; j <= _afterFieldsRecordNames.Count; j++)
                {
                    var row = new Row {RowIndex = ++rowIndex};

                    newCell = row.InsertAt(new Cell(), 0);
                    newCell.CellValue = new CellValue(string.Empty);
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    newCell = row.InsertAt(new Cell(), 1);
                    newCell.CellValue = new CellValue($"{_afterFieldsRecordNames[j - 1]}");
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                    sheetData.Append(row);
                }

                for (var rowNumber = 3;
                    rowNumber <= _beforeFieldsRecordNames.Count + _fieldRecordNames.Count*simulationInfo.FieldNames.Count +
                    _afterFieldsRecordNames.Count + 2;
                    rowNumber++)
                {
                    var row = sheetData.ChildElements[rowNumber - 1];

                    for (var dayNumber = 1;
                        dayNumber <= simulationInfo.SeasonCount * (simulationInfo.DaysCount + 1);
                        dayNumber++)
                    {
                        newCell = row.InsertAt(new Cell(), dayNumber + 1);
                        newCell.CellValue = new CellValue();
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    }
                }

                spreadsheetDocument.Save();
            }
        }

        private Stylesheet CreateStylesheet()
        {
            var stylesheet = new Stylesheet();

            stylesheet.NumberingFormats = new NumberingFormats();

            var nf2decimal = new NumberingFormat
            {
                NumberFormatId = UInt32Value.FromUInt32(3453), 
                FormatCode = StringValue.FromString("0.00")
            };
            stylesheet.NumberingFormats.Append(nf2decimal);

            var cellFormat = new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                NumberFormatId = nf2decimal.NumberFormatId,
                ApplyNumberFormat = BooleanValue.FromBoolean(true),
                ApplyFont = true
            };
            stylesheet.CellFormats.AppendChild(cellFormat);

            stylesheet.CellFormats.Count = UInt32Value.FromUInt32((uint)stylesheet.CellFormats.ChildElements.Count);

            stylesheet.Save();

            return stylesheet;
        }

        private Point GetRecordCoordinate(AgroHydrologyRecord record, SimulationInfo simulationInfo)
        {
            if (record is AgroHydrologyFieldRecord fieldRecord)
                return new Point(
                    (fieldRecord.Season - 1) * simulationInfo.DaysCount + fieldRecord.Day + 1 +
                    (fieldRecord.Season - 1),
                    _beforeFieldsRecordNames.Count +
                    simulationInfo.FieldNames.IndexOf(fieldRecord.Field) * _fieldRecordNames.Count +
                    _fieldRecordNames.IndexOf(fieldRecord.RecordName) + 2);

            if (record is AgroHydrologyDayRecord dayRecord)
            {
                if (_beforeFieldsRecordNames.Contains(dayRecord.RecordName))
                    return new Point(
                        (dayRecord.Season - 1) * simulationInfo.DaysCount + dayRecord.Day + 1 + (dayRecord.Season - 1),
                        _beforeFieldsRecordNames.IndexOf(dayRecord.RecordName) + 2);

                if (_afterFieldsRecordNames.Contains(dayRecord.RecordName))
                    return new Point(
                        (dayRecord.Season - 1) * simulationInfo.DaysCount + dayRecord.Day + 1 + (dayRecord.Season - 1),
                        _beforeFieldsRecordNames.Count + simulationInfo.FieldNames.Count * _fieldRecordNames.Count +
                        _afterFieldsRecordNames.IndexOf(dayRecord.RecordName) + 2);
            }

            if (_afterFieldsRecordNames.Contains(record.RecordName))
                return new Point(record.Season * simulationInfo.DaysCount + 1 + (record.Season - 1) + 1,
                    _beforeFieldsRecordNames.Count + simulationInfo.FieldNames.Count * _fieldRecordNames.Count +
                    _afterFieldsRecordNames.IndexOf(record.RecordName) + 2);

            throw new ArgumentException(nameof(record));
        }

        private void SaveAgroHydrologyResults(string path, SimulationInfo simulationInfo,
            IEnumerable<AgroHydrologyRecord> records)
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Open(path, true))
            {
                var workbook = spreadsheetDocument.WorkbookPart.Workbook;
                var worksheetPart = workbook.WorkbookPart.WorksheetParts.First();
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                foreach (var record in records)
                {
                    var coordinates = GetRecordCoordinate(record, simulationInfo);

                    var row = sheetData.ChildElements[coordinates.Y];

                    var cell = (Cell) row.ChildElements[coordinates.X];
                    cell.DataType = GetCellValues(record);

                    cell.CellValue = double.TryParse(record.Value.ToString(), out var value)
                        ? new CellValue($"{value:F2}")
                        : new CellValue(record.Value.ToString());
                }
            }
        }

        private CellValues GetCellValues(AgroHydrologyRecord record)
        {
            if (record.RecordName.Equals(SimulationInfo.Plant))
                return CellValues.String;

            return CellValues.String;
        }

        #endregion
    }
}