using System.Collections.Generic;
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

        public void AddFieldRecord(int season, int day, string field, string property, string value)
        {
            var record = new AgroHydrologyFieldRecord(season, day, field, property, value);
            _records.Add(record);

            ProcessRecords();
        }

        public void AddRecord(int season, int day, string property, string value)
        {
            var record = new AgroHydrologyRecord(season, day, property, value);
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
            switch (_saveFrequency)
            {
                case SaveFrequency.PerSeason:
                    if (_records.Count > 2 &&
                        _records[_records.Count - 1].Season != _records[_records.Count - 2].Season)
                        Save();
                    break;
                case SaveFrequency.PerDay:
                    if (_records.Count > 2 &&
                        _records[_records.Count - 1].Day != _records[_records.Count - 2].Day)
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