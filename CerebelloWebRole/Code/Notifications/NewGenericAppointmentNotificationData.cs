using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Notifications
{
    /// <summary>
    /// Data about a notification that is sent to the client
    /// </summary>
    public class NewGenericAppointmentNotificationData
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
        /// Doctor Id
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// Doctor Name
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// Appointment time
        /// E.g: 18:00h
        /// </summary>
        public string AppointmentTime { get; set; }

        /// <summary>
        /// The URL to be called to make the appointment accomplished
        /// </summary>
        public string AppointmentDiscardedUrl { get; set; }

        /// <summary>
        /// The URL to be called to make the appointment polled
        /// </summary>
        public string AppointmentIsPolledUrl { get; set; }

        /// <summary>
        /// Appointment description
        /// </summary>
        public string Description { get; set; }
    }
}