using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CerebelloWebRole.Code.Helpers;

namespace CerebelloWebRole.Code.Notifications
{
    public class NotificationsHelper
    {
        /// <summary>
        /// Returns all the notifications for the given user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <param name="controller"> </param>
        /// <param name="polled"></param>
        /// <returns></returns>
        public static List<NotificationData> GetNotifications(CerebelloEntitiesAccessFilterWrapper db, int userId, Controller controller, bool polled)
        {
            var result = new List<NotificationData>();
            var notifications = db.Notifications.Where(n => n.UserId == userId && n.IsPolled == polled);

            if (notifications.Any())
            {
                foreach (var notification in notifications)
                {
                    string text;
                    if (string.IsNullOrWhiteSpace(notification.ViewName))
                    {
                        text = notification.Text;
                    }
                    else
                    {
                        var jsonData = System.Web.Helpers.Json.Decode(notification.ViewData);
                        var viewDataDic = new ViewDataDictionary(jsonData);
                        text = MvcHelper.RenderPartialViewToString(controller.ControllerContext, notification.ViewName, viewDataDic);
                    }

                    result.Add(new NotificationData()
                        {
                            NotificationId = notification.Id,
                            NotificationClientId = "notification_" + notification.Id,
                            Text = text,
                            NotificationIsPolledUrl =
                                controller.Url.Action("SetNotificationAsPolled", "Notifications", new { id = notification.Id }),
                            NotificationRemoveUrl = controller.Url.Action("RemoveNotification", "Notifications", new { id = notification.Id })
                        });
                }
            }

            return result;
        }
    }
}
