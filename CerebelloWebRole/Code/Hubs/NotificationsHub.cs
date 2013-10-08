using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CerebelloWebRole.Code
{
    public class NotificationsHub : CerebelloHub
    {
        /// <summary>
        /// Current connections
        /// 1 room has many users that have many connections (2 open browsers from the same user represents 2 connections)
        /// </summary>
        private static readonly Dictionary<string, Dictionary<int, List<string>>> connections = new Dictionary<string, Dictionary<int, List<string>>>();

        public void PatientArrived(int appointmentId, string time)
        {
            var appointment = this.db.Appointments.FirstOrDefault(p => p.Id == appointmentId);
            if (appointment == null)
                return;

            appointment.Status = (int)TypeAppointmentStatus.Accomplished;

            Debug.Assert(appointment.PatientId != null, "appointment.PatientId != null");
            var notificationData = new PatientArrivedNotificationData()
                {
                    PatientId = appointment.PatientId.Value,
                    PatientName = PersonHelper.GetFullName(appointment.Patient.Person),
                    Time = time,
                    PracticeIdentifier = appointment.Practice.UrlIdentifier,
                    DoctorIdentifier = appointment.Doctor.UrlIdentifier
                };

            var notificationDataString = new JavaScriptSerializer().Serialize(notificationData);

            var newNotification = new Notification()
                {
                    CreatedOn = DateTime.UtcNow,
                    IsClosed = false,
                    UserToId = appointment.DoctorId,
                    Type = NotificationConstants.PATIENT_ARRIVED_NOTIFICATION_TYPE,
                    PracticeId = appointment.PracticeId,
                    Data = notificationDataString
                };

            this.db.Notifications.AddObject(newNotification);

            this.db.SaveChanges();

            BroadcastDbNotification(newNotification, notificationData);
        }


        public void PatientWillNotArrive(int appointmentId)
        {
            var appointment = this.db.Appointments.FirstOrDefault(p => p.Id == appointmentId);
            if (appointment == null)
                return;
            appointment.Status = (int)TypeAppointmentStatus.NotAccomplished;
            this.db.SaveChanges();
        }

        public void CloseNotification(int notificationId)
        {
            var notification = this.db.Notifications.FirstOrDefault(p => p.Id == notificationId);
            if (notification == null)
                return;
            notification.IsClosed = true;
            this.db.SaveChanges();
        }

        /// <summary>
        /// Returns my user id
        /// </summary>
        /// <returns></returns>
        private int GetMyUserId()
        {
            var userPrincipal = this.Context.User as AuthenticatedPrincipal;
            if (userPrincipal == null)
                throw new NotAuthorizedException();

            var userData = userPrincipal.Profile;
            return userData.Id;
        }

        private string GetMyRoomId()
        {
            var userPrincipal = this.Context.User as AuthenticatedPrincipal;
            if (userPrincipal == null)
                throw new NotAuthorizedException();

            var userData = userPrincipal.Profile;
            return userData.PracticeIdentifier;
        }

        public override Task OnConnected()
        {
            var myRoomId = this.GetMyRoomId();
            var myUserId = this.GetMyUserId();

            lock (connections)
            {
                if (!connections.ContainsKey(myRoomId))
                    connections[myRoomId] = new Dictionary<int, List<string>>();

                if (!connections[myRoomId].ContainsKey(myUserId))
                    connections[myRoomId][myUserId] = new List<string>();

                connections[myRoomId][myUserId].Add(this.Context.ConnectionId);
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            var myRoomId = this.GetMyRoomId();
            var myUserId = this.GetMyUserId();

            lock (connections)
            {
                if (connections.ContainsKey(myRoomId))
                    if (connections[myRoomId].ContainsKey(myUserId))
                        if (connections[myRoomId][myUserId].Contains(this.Context.ConnectionId))
                        {
                            connections[myRoomId][myUserId].Remove(this.Context.ConnectionId);
                            if (!connections[myRoomId][myUserId].Any())
                                connections[myRoomId].Remove(myUserId);
                        }
            }

            return base.OnDisconnected();
        }

        /// <summary>
        /// Dispatches database notifications to the clients.
        /// THIS METHOD CAN BE CALLED FROM ANYWHERE
        /// </summary>
        /// <param name="notifications"></param>
        public static void BroadcastDbNotifications([NotNull] IEnumerable<Tuple<Notification, object>> notifications)
        {
            if (notifications == null) throw new ArgumentNullException("notifications");

            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

            // we ITERATE TWICE, because we can't SEND the message in this iteration because of this lock. It would be too slow.
            // we better iterate again and send the messages
            lock (connections)
            {
                foreach (var notificationToBeDispatched in notifications)
                {
                    var notification = notificationToBeDispatched.Item1;
                    var notificationData = notificationToBeDispatched.Item2;
                    // verify which client connections should receive this notification
                    if (connections.ContainsKey(notification.Practice.UrlIdentifier))
                        if (connections[notification.Practice.UrlIdentifier].ContainsKey(notification.UserToId))
                        {
                            foreach (var connectionId in connections[notification.Practice.UrlIdentifier][notification.UserToId])
                                context.Clients.Client(connectionId).notify(notification.Id, notification.Type, notificationData);
                        }
                }
            }
        }

        /// <summary>
        /// Dispatches database notifications to the clients.
        /// THIS METHOD CAN BE CALLED FROM ANYWHERE
        /// </summary>
        public static void BroadcastDbNotification([NotNull] Notification dbNotification, [NotNull] object notificationData)
        {
            if (dbNotification == null) throw new ArgumentNullException("dbNotification");
            if (notificationData == null) throw new ArgumentNullException("notificationData");
            BroadcastDbNotifications(new List<Tuple<Notification, object>> { new Tuple<Notification, object>(dbNotification, notificationData) });
        }
    }
}