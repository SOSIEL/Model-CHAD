using System.Collections.Generic;

namespace Model
{
    public interface ILogger : IEnumerable<LogEntry>
    {
        #region Public Interface

        void Write(string text);

        void Clear();

        #endregion
    }
}