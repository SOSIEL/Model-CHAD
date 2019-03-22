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


        private readonly FileStream _fileStream;

        #endregion

        #region Constructors

        public FileLogger(string folderPath)
        {
            _path = Path.Combine(folderPath, SimulationLogsName);

            var directoryInfo = new DirectoryInfo(folderPath);

            if (!directoryInfo.Exists)
                directoryInfo.Create();

            _fileStream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            _streamWriter = new StreamWriter(_fileStream);
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

                    yield return new LogEntry(time, text);
                }
            }
        }

        public void Write(string text)
        {
            _streamWriter.WriteLine($"{DateTimeOffset.Now.TimeOfDay} :: {text}");
        }

        public void Clear()
        {
            using (new FileStream(_path, FileMode.Create, FileAccess.Write))
            {
            }
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
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