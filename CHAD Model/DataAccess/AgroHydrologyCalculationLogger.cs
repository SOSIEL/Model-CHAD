using System.Collections.Generic;
using System.Linq;
using CHAD.Model.Infrastructure;

namespace CHAD.DataAccess
{
    public class AgroHydrologyCalculationLogger : IAgroHydrologyCalculationLogger
    {
        #region Fields

        private readonly List<AgroHydrologyRecord> _records;
        private readonly SaveFrequency _saveFrequency;
        private readonly SimulationInfo _simulationInfo;
        private readonly IStorageService _storageService;

        #endregion

        #region Constructors

        public AgroHydrologyCalculationLogger(IStorageService storageService, SaveFrequency saveFrequency,
            SimulationInfo simulationInfo)
        {
            _storageService = storageService;
            _saveFrequency = saveFrequency;
            _simulationInfo = simulationInfo;

            _records = new List<AgroHydrologyRecord>();
        }

        #endregion

        #region Public Interface

        public void AddRecord(int season, int day, string field, string property, object value)
        {
            var record = new AgroHydrologyFieldRecord(season, day, field, property, value);
            _records.Add(record);

            ProcessRecords();
        }

        public void AddRecord(int season, int day, string property, object value)
        {
            var record = new AgroHydrologyDayRecord(season, day, property, value);
            _records.Add(record);

            ProcessRecords();
        }

        public void AddRecord(int season, string property, object value)
        {
            var record = new AgroHydrologyRecord(season,property, value);
            _records.Add(record);

            ProcessRecords();
        }


        public void Complete()
        {
            Save();
        }

        #endregion

        #region All other members

        private void ProcessRecords()
        {
            if(!_records.Any())
                return;

            switch (_saveFrequency)
            {
                case SaveFrequency.PerRecord:
                    if(_saveFrequency == SaveFrequency.PerRecord)
                        Save();
                    break;
                case SaveFrequency.PerDay:
                    if(_records.Count == 1)
                        return;

                    var preLast = _records[_records.Count - 2];
                    var last = _records[_records.Count - 1];

                    if (preLast is AgroHydrologyDayRecord preLastDayRecord && last is AgroHydrologyDayRecord lastDayRecord && preLastDayRecord.Day != lastDayRecord.Day)
                        Save();
                    break;
                case SaveFrequency.PerSeason:
                    if(_records.Count == 1)
                        return;

                    var preLastRecord = _records[_records.Count - 2];
                    var lastRecord = _records[_records.Count - 1];

                    if (preLastRecord.Season != lastRecord.Season)
                        Save();
                    break;
            }
        }

        private void Save()
        {
            _storageService.SaveAgroHydrologyResults(_simulationInfo, _records);

            _records.Clear();
        }

        #endregion
    }
}