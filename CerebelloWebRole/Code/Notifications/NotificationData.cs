using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Notifications
{
    public class NotificationData
    {
        /// <summary>
        /// This is the Id of the object in the database.
        /// E.g: 124 or 145
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        /// This Id is the Id of the notification in the client, not the Id of the notification object in the server
        /// Everytime a notification is displayed in the client, it may have an Id to avoid duplicates.
        /// E.g: "appointment_124" or "appointment_145"
        /// </summary>
        public string NotificationClientId { get; set; }

        /// <summary>
        /// The actual body of the notification.
        /// This text SUPPORTS HTML so that you can create links
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The URL to be called to make the notification polled
        /// </summary>
        public string NotificationIsPolledUrl { get; set; }

        /// <summary>
        /// The URL to be called to close the notification and remove it from the database
        /// </summary>
        public string NotificationRemoveUrl { get; set; }
    }
}