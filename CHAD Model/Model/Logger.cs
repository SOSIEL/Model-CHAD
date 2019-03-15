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

        #endregion
    }
}