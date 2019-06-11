using System;
using System.Collections.Generic;
using System.Drawing;
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
        private const string HydrologyDetailedOutput = "HydrologyDetailed.xlsx";

        private readonly List<string> _beforeFieldsRecordNames = new List<string> {SimulationInfo.Precip, SimulationInfo.SnowInSnowpack};

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
            SimulationInfo.EvapTransToDate,
            SimulationInfo.Harvestable
        };

        private readonly List<string> _afterFieldsRecordNames = new List<string> {SimulationInfo.LeakAquifer, SimulationInfo.WaterInAquifer3, SimulationInfo.WaterInAquiferChange};

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

        private void CreateAgroHydrologyDetailedFile(string path, SimulationInfo simulationInfo)
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

                for (int i = 1; i <= simulationInfo.SeasonCount; i++)
                {
                    for (int j = 1; j <= simulationInfo.DaysCount; j++)
                    {
                        newCell = row1.InsertAt(new Cell(), (i - 1) * simulationInfo.DaysCount + j + 1);
                        newCell.CellValue = new CellValue($"Season {i}");
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                        newCell = row2.InsertAt(new Cell(), (i - 1) * simulationInfo.DaysCount + j + 1);
                        newCell.CellValue = new CellValue($"Day {j}");
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    }
                }

                var rowIndex = 2u;

                for (int j = 1; j <= _beforeFieldsRecordNames.Count; j++)
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

                for (int i = 1; i <= simulationInfo.FieldNames.Count; i++)
                {
                    for (int j = 1; j <= _fieldRecordNames.Count; j++)
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
                }

                for (int j = 1; j <= _afterFieldsRecordNames.Count; j++)
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


                spreadsheetDocument.Save();
            }
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

                    var newCell = row.InsertAt(new Cell(), coordinates.X);
                    newCell.CellValue = new CellValue($"{record.Value}");
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                }
            }
        }

        private Point GetRecordCoordinate(AgroHydrologyRecord record, SimulationInfo simulationInfo)
        {
            if (record is AgroHydrologyFieldRecord fieldRecord)
            {
                return new Point((fieldRecord.Season - 1) * simulationInfo.DaysCount + fieldRecord.Day + 1,
                    _beforeFieldsRecordNames.Count + simulationInfo.FieldNames.IndexOf(fieldRecord.Field) * _fieldRecordNames.Count +
                    _fieldRecordNames.IndexOf(fieldRecord.RecordName) + 2);
            }

            if (_beforeFieldsRecordNames.Contains(record.RecordName))
            {
                return new Point((record.Season - 1) * simulationInfo.DaysCount + record.Day + 1,
                    _beforeFieldsRecordNames.IndexOf(record.RecordName) + 2);
            }

            if (_afterFieldsRecordNames.Contains(record.RecordName))
            {
                return new Point((record.Season - 1) * simulationInfo.DaysCount + record.Day + 1,
                    _beforeFieldsRecordNames.Count + simulationInfo.FieldNames.Count * _fieldRecordNames.Count +
                    _afterFieldsRecordNames.IndexOf(record.RecordName) + 2);
            }

            throw new ArgumentException(nameof(record));
        }
    }
}