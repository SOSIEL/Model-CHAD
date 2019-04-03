using System;

namespace CHAD.Model.Infrastructure
{
    public class LogEntry
    {
        #region Constructors

        public LogEntry(string text, Severity severity)
            : this(DateTimeOffset.Now, text, severity)
        {
        }

        public LogEntry(DateTimeOffset time, string text, Severity severity)
        {
            Time = time;
            Text = text;
            Severity = severity;
        }

        #endregion

        #region Public Interface

        public DateTimeOffset Time { get; }

        public string Text { get; }
        public Severity Severity { get; }

        #endregion
    }
}