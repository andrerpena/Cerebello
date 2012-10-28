using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cerebello.Model;

namespace CerebelloWebRole.Code.LongPolling
{
    public abstract class LongPollingProvider
    {
        public const int WAIT_TIMEOUT = 1000000;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="practiceId"></param>
        /// <param name="timestamp"></param>
        /// <param name="db"></param>
        /// <param name="onEvents"></param>
        public abstract List<LongPollingEvent> WaitForEvents(int userId, int practiceId, long timestamp, CerebelloEntities db);
    }
}