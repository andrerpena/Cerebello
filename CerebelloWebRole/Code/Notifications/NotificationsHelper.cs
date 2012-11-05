using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Notifications
{
    public class NotificationsHelper
    {
        /// <summary>
        /// Returns all the notifications for the given user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <param name="polled"></param>
        /// <returns></returns>
        public static List<NotificationData> GetNotifications(CerebelloEntitiesAccessFilterWrapper db, int userId, UrlHelper url, bool polled)
        {
            var result = new List<NotificationData>();
            var notifications = db.Notifications.Where(n => n.UserId == userId && n.IsPolled == polled);

            if (notifications.Any())
            {
                foreach (var notification in notifications)
                {
                    result.Add(new NotificationData()
                        {
                            NotificationId = notification.Id,
                            NotificationClientId = "notification_" + notification.Id,
                            Text = notification.Text,
                            NotificationIsPolledUrl =
                                url.Action("SetNotificationAsPolled", "Notifications", new { id = notification.Id }),
                            NotificationRemoveUrl = url.Action("RemoveNotification", "Notifications", new { id = notification.Id })
                        });
                }
            }

            return result;
        }
    }
}