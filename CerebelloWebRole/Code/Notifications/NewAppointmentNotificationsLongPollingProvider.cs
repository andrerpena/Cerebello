﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.LongPolling;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Notifications
{
    public class NewAppointmentNotificationsLongPollingProvider : LongPollingProvider
    {
        private Func<DateTime> utcNowGetter = null;

        public override void Initialize()
        {
            if (this.utcNowGetter == null)
                this.utcNowGetter = () => DateTime.UtcNow;
        }

        public override IEnumerable<LongPollingEvent> WaitForEvents(int userId, int practiceId, long timestamp,
                                                                    [NotNull] string connectionString, UrlHelper url)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            using (var db = new CerebelloEntitiesAccessFilterWrapper(new CerebelloEntities(connectionString)))
            {
                db.SetCurrentUserById(userId);

                var result = new List<LongPollingEvent>();
                var notificationData = NewAppointmentNotificationsHelper.GetNewAppointmentNotifications(db, practiceId, userId, this.utcNowGetter, url, false);

                if (notificationData.Any())
                {
                    foreach (var notification in notificationData)
                        result.Add(new LongPollingEvent()
                            {
                                ProviderName = "new-appointment",
                                EventKey = notification.NotificationClientId,
                                Data = notification
                            });
                }
                return result;
            }
        }
    }
}