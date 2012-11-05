using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.LongPolling;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Notifications
{
    public class NotificationsLongPollingProvider : LongPollingProvider
    {
        public override void Initialize()
        {
            
        }

        public override IEnumerable<LongPollingEvent> WaitForEvents(int userId, int practiceId, long timestamp,
                                                                    [NotNull] string connectionString,
                                                                    [NotNull] UrlHelper url)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (url == null) throw new ArgumentNullException("url");

            using (var db = new CerebelloEntitiesAccessFilterWrapper(new CerebelloEntities(connectionString)))
            {
                db.SetCurrentUserById(userId);

                var result = new List<LongPollingEvent>();
                var notificationData = NotificationsHelper.GetNotifications(db, userId, url, false);

                if (notificationData.Any())
                {
                    foreach (var notification in notificationData)
                        result.Add(new LongPollingEvent()
                        {
                            ProviderName = "notifications",
                            EventKey = notification.NotificationClientId,
                            Data = notification
                        });
                }
                return result;
            }
        }
    }
}