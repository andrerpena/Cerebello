using System.Collections.Generic;

namespace CerebelloWebRole.Code.LongPolling
{
    public abstract class LongPollingProvider
    {
        public const int WAIT_TIMEOUT = 30000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="practiceId"></param>
        /// <param name="timestamp"></param>
        /// <param name="db"></param>
        public abstract IEnumerable<LongPollingEvent> WaitForEvents(int userId, int practiceId, long timestamp, CerebelloEntitiesAccessFilterWrapper db);
    }
}
