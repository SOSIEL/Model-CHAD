using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CHAD.Model.Infrastructure;

namespace CHAD.DataAccess
{
    public sealed class FileLogger : ILogger, IDisposable
    {
        #region Static Fields and Constants

        private const string Separator = " :: ";

        private const string SimulationLogsName = "Logs.txt";

        #endregion

        #region Fields

        private readonly string _path;
        private readonly StreamWriter _streamWriter;

        #endregion

        #region Constructors

        public FileLogger(string folderPath)
        {
            _path = Path.Combine(folderPath, SimulationLogsName);

            var directoryInfo = new DirectoryInfo(folderPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var fileStream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            _streamWriter = new StreamWriter(fileStream);
        }

        #endregion

        #region Public Interface

        public IEnumerator<LogEntry> GetEnumerator()
        {
            using (var fileStream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var streamReader = new StreamReader(fileStream))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        yield break;

                    var separatorIndex = line.IndexOf(" :: ", StringComparison.Ordinal);
                    var timeString = line.Substring(0, separatorIndex - 1);
                    var text = line.Substring(0, timeString.Length + Separator.Length);

                    var time = DateTimeOffset.Parse(timeString);

                    yield return new LogEntry(time, text, Severity.Level1);
                }
            }
        }

        public void Write(string text, Severity severity)
        {
            var severityString =
                severity == Severity.Level1 ? string.Empty : severity == Severity.Level2 ? "\t" : "\t\t";
            _streamWriter.WriteLine($"{DateTimeOffset.Now.TimeOfDay} :: {severityString}{text}");
        }

        public void Clear()
        {
            using (new FileStream(_path, FileMode.Create, FileAccess.Write))
            {
            }
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }

        #endregion

        #region Interface Implementations

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}