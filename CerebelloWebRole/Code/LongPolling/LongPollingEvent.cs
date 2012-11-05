using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.LongPolling
{
    public class LongPollingEvent
    {
        public string ProviderName { get; set; }

        /// <summary>
        /// This can either be the Id of the event or represent a provider sub-category.
        /// </summary>
        public string EventKey { get; set; }
        public object Data { get; set; }
    }
}