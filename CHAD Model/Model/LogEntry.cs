using System;

namespace Model
{
    public class LogEntry
    {
        public LogEntry(string text)
        {
            Time = DateTimeOffset.Now;
            Text = text;
        }

        public DateTimeOffset Time { get; }

        public string Text { get; }
    }
}
