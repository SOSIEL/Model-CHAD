using System.Collections;
using System.Collections.Generic;

namespace Model
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

        public void Write(string text)
        {
            Entries.AddLast(new LogEntry(text));
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