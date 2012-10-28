using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.LongPolling
{
    public class LongPollingEvent
    {
        public string ProviderName { get; set; }
        public string EventKey { get; set; }
        public object Data { get; set; }
    }
}