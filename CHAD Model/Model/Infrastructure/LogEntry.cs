using System;

namespace Model
{
    public class LogEntry
    {
        #region Constructors

        public LogEntry(string text)
            : this(DateTimeOffset.Now, text)
        {
        }

        public LogEntry(DateTimeOffset time, string text)
        {
            Time = time;
            Text = text;
        }

        #endregion

        #region Public Interface

        public DateTimeOffset Time { get; }

        public string Text { get; }

        #endregion
    }
}