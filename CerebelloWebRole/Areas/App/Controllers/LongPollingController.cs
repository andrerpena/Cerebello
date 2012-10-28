using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Chat;
using CerebelloWebRole.Code.LongPolling;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class LongPollingController : PracticeController
    {
        private readonly List<LongPollingProvider> providers = new List<LongPollingProvider>();

        public LongPollingController()
        {
            // register providers here
            this.providers.Add(new ChatLongPollingProvider());
        }

        /// <summary>
        /// Returns long polling evetns to the client.
        /// If there are not events right now. This action will STOP and wait for them
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetEvents(long timeStamp)
        {
            var practiceId = this.Practice.Id;
            var userId = this.DbUser.Id;

            var eventsReturned = new List<LongPollingEvent>();
            var eventsReturnedLock = new object();

            var wait = new AutoResetEvent(false);
            foreach (var provider in this.providers)
            {
                // for each provider, let's create a different thread that will listen to that particular kind
                // of event. When the first returns, this Action returns as well
                ThreadPool.QueueUserWorkItem(providerClosure =>
                    {
                        var events = ((LongPollingProvider)providerClosure).WaitForEvents(userId, practiceId, timeStamp, this.db);
                        lock (eventsReturnedLock)
                        {
                            eventsReturned.AddRange(events);
                            wait.Set();
                        }
                    }, provider);
            }
            wait.WaitOne(LongPollingProvider.WAIT_TIMEOUT);

            return this.Json(new
            {
                Events = eventsReturned,
                Timestamp = DateTime.UtcNow.Ticks.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}