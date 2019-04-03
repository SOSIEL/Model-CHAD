using System.Collections;
using System.Collections.Generic;

namespace CHAD.Model.Infrastructure
{
    public class SimpleLogger : ILogger
    {
        #region Constructors

        public SimpleLogger()
        {
            Entries = new LinkedList<LogEntry>();
        }

        #endregion

        #region Public Interface

        public LinkedList<LogEntry> Entries { get; }

        public void Write(string text, Severity severity)
        {
            Entries.AddLast(new LogEntry(text, severity));
        }

        public void Clear()
        {
            Entries.Clear();
        }

        public IEnumerator<LogEntry> GetEnumerator()
        {
            return Entries.GetEnumerator();
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