using System.Collections.Generic;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.LongPolling
{
    public abstract class LongPollingProvider
    {
        public const int WAIT_TIMEOUT = 30000;

        /// <summary>
        /// Initializes the long polling provider
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="practiceId"></param>
        /// <param name="timestamp"></param>
        /// <param name="connectionString"></param>
        /// <param name="url"></param>
        public abstract IEnumerable<LongPollingEvent> WaitForEvents(int userId, int practiceId, long timestamp, string connectionString, UrlHelper url);
    }
}
