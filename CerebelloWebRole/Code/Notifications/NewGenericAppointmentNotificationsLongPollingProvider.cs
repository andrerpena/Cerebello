using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Access;
using CerebelloWebRole.Code.LongPolling;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Notifications
{
    public class NewGenericAppointmentNotificationsLongPollingProvider : LongPollingProvider
    {
        private Func<DateTime> utcNowGetter = null;

        public override void Initialize()
        {
            if (this.utcNowGetter == null)
                this.utcNowGetter = () => DateTime.UtcNow + DebugConfig.CurrentTimeOffset;
        }

        public override IEnumerable<LongPollingEvent> WaitForEvents(
            int userId,
            int practiceId,
            long timestamp,
            [NotNull] string connectionString,
            Controller controller)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            using (var db = new CerebelloEntitiesAccessFilterWrapper(new CerebelloEntities(connectionString)))
            {
                db.SetCurrentUserById(userId);

                var result = new List<LongPollingEvent>();
                var notificationData = NewAppointmentNotificationsHelper.GetNewGenericAppointmentNotifications(
                    db, practiceId, userId, this.utcNowGetter, controller.Url, false);

                if (notificationData.Any())
                {
                    foreach (var notification in notificationData)
                        result.Add(
                            new LongPollingEvent()
                                {
                                    ProviderName = "new-generic-appointment",
                                    EventKey = notification.NotificationClientId,
                                    Data = notification
                                });
                }
                return result;
            }
        }
    }
}