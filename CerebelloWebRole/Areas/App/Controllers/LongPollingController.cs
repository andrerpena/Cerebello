using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Chat;
using CerebelloWebRole.Code.LongPolling;
using CerebelloWebRole.Code.Notifications;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class LongPollingController : PracticeController
    {
        private readonly List<LongPollingProvider> providers = new List<LongPollingProvider>();

        public LongPollingController()
        {
            // register providers here
            this.providers.Add(new ChatLongPollingProvider());
            this.providers.Add(new NewAppointmentNotificationsLongPollingProvider());
            this.providers.Add(new NotificationsLongPollingProvider());

            foreach (var provider in this.providers)
                provider.Initialize();
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

            // the current connection string [DO NOT PUT THIS CODE INSIDE THE ThreadPool.QueueUserWorkItem. Leave it here]
            var connectionString = this.db.Connection.ConnectionString;

            foreach (var provider in this.providers)
            {
                // for each provider, let's create a different thread that will listen to that particular kind
                // of event. When the first returns, this Action returns as well
                ThreadPool.QueueUserWorkItem(providerClosure =>
                    {
                        try
                        {
                            var events = ((LongPollingProvider)providerClosure).WaitForEvents(userId, practiceId, timeStamp, connectionString, this.Url);
                            // if the provider returned no events, we're not going to continue. Maybe another provider will return
                            // something later on (the new appointment provider returns immediately when no appointments exists, without
                            // this IF, it would always stop the longpolling without waiting for the chat.)
                            if (events.Any())
                                lock (eventsReturnedLock)
                                {
                                    eventsReturned.AddRange(events);
                                    wait.Set();
                                }
                        }
                        catch
                        {
                            // The long polling cannot stop because a provider triggered an exception
                            // todo: log exception here
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