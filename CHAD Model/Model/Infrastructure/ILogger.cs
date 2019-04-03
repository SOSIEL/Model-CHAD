using System.Collections.Generic;

namespace CHAD.Model.Infrastructure
{
    public interface ILogger : IEnumerable<LogEntry>
    {
        #region Public Interface

        void Write(string text, Severity severity);

        void Clear();

        #endregion
    }
}