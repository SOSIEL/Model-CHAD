using System.Collections;
using System.Collections.Generic;

namespace CHAD.Model.Infrastructure
{
    public class DummyLogger : ILogger
    {
        #region Public Interface

        public IEnumerator<LogEntry> GetEnumerator()
        {
            yield break;
        }

        public void Write(string text)
        {
        }

        public void Clear()
        {
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