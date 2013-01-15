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
    public class NewMedicalAppointmentNotificationsLongPollingProvider : LongPollingProvider
    {
        private Func<DateTime> utcNowGetter = null;

        public override void Initialize()
        {
            if (this.utcNowGetter == null)
                this.utcNowGetter = () => DateTime.UtcNow;
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
                var notificationData = NewAppointmentNotificationsHelper.GetNewMedicalAppointmentNotifications(
                    db, practiceId, userId, controller.Url, false);

                if (notificationData.Any())
                {
                    foreach (var notification in notificationData)
                        result.Add(
                            new LongPollingEvent()
                                {
                                    ProviderName = "new-medical-appointment",
                                    EventKey = notification.NotificationClientId,
                                    Data = notification
                                });
                }
                return result;
            }
        }
    }
}