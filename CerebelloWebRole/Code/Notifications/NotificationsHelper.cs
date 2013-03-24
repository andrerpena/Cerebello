using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.Script.Serialization;
using Cerebello.Model;
using System.Linq;
using CerebelloWebRole.Code.Hubs;
using CerebelloWebRole.Code.Notifications.Data;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;

namespace CerebelloWebRole.Code.Notifications
{
    public static class NotificationsHelper
    {
        public static void CreateNotificationsJob()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
                {
                    try
                    {
                        while (true)
                        {
                            using (var db = new CerebelloEntities())
                            {
                                // this TUPLE is: Notification AND NotificationData (which is SPECIFIC for each type of notification)
                                var notificationsToBeDispatched = new List<Tuple<Notification, object>>();

                                // events should be GREATER OR EQUAL to (referenceDate) and LESSER than (referenceDate + offset)
                                var referenceTime = DateTime.UtcNow;

                                // check for new medical appointments
                                try
                                {
                                    GenerateNotificationsForNewMedicalAppointments(db, referenceTime, notificationsToBeDispatched);
                                }
                                catch (Exception)
                                {
                                    // log error
                                }

                                // check for generic appointments
                                try
                                {
                                    GenerateNotificationsForNewGenericAppointments(db, referenceTime, notificationsToBeDispatched);
                                }
                                catch (Exception)
                                {
                                    // log error
                                }

                                // dispatch notifications to the client
                                NotificationsHub.BroadcastDbNotifications(notificationsToBeDispatched);
                            }

                            Thread.Sleep(5 * 60 * 1000);
                        }
                    }
                    catch
                    {
                        // exceptions cannot be unhandled or the service will stop
                    }
                }), null);
        }

        /// <summary>
        /// Generates notifications for new Medical Appointments
        /// </summary>
        private static void GenerateNotificationsForNewMedicalAppointments(
            [NotNull] CerebelloEntities db, DateTime referenceTime, [NotNull] ICollection<Tuple<Notification, object>> notificationsToBeDispatched)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (notificationsToBeDispatched == null) throw new ArgumentNullException("notificationsToBeDispatched");

            // check for appointments that have to be notified
            var timeOffset = referenceTime.AddMinutes(10);
            var unnotifiedAppointments =
                db.Appointments.Where(
                    a => !a.Notified && a.Type == (int)TypeAppointment.MedicalAppointment && a.Start >= referenceTime && a.Start < timeOffset).ToList();
            foreach (var appointment in unnotifiedAppointments)
            {
                Debug.Assert(appointment.PatientId != null, "appointment.PatientId != null");

                var medicalAppointmentData = new MedicalAppointmentNotificationData
                {
                    PatientId = appointment.PatientId.Value,
                    PatientName = appointment.Patient.Person.FullName,
                    DoctorName = appointment.Patient.Doctor.Users.First().Person.FullName,
                    DoctorId = appointment.Patient.DoctorId,
                    AppointmentId = appointment.Id,
                    Time = DateTimeHelper.GetFormattedTime(
                        PracticeController.ConvertToLocalDateTime(appointment.Practice, appointment.Start)),
                    PracticeIdentifier = appointment.Practice.UrlIdentifier,
                    DoctorIdentifier = appointment.Doctor.UrlIdentifier
                };

                var medicalAppointmentDataString = new JavaScriptSerializer().Serialize(medicalAppointmentData);

                // for each secretary, I need to create a new notification
                foreach (var user in appointment.Practice.Users.Where(user => user.Secretary != null))
                {
                    var newNotification = new Notification()
                    {
                        CreatedOn = referenceTime,
                        IsClosed = false,
                        UserToId = user.Id,
                        Type = NotificationConstants.MEDICAL_APPOINTMENT_NOTIFICATION_TYPE,
                        PracticeId = appointment.PracticeId,
                        Data = medicalAppointmentDataString
                    };

                    user.Notifications.Add(newNotification);
                    notificationsToBeDispatched.Add(new Tuple<Notification, object>(newNotification, medicalAppointmentData));
                }

                appointment.Notified = true;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Generates notifications for new Medical Appointments
        /// </summary>
        private static void GenerateNotificationsForNewGenericAppointments(
            [NotNull] CerebelloEntities db, DateTime referenceTime)
        {
            GenerateNotificationsForNewGenericAppointments(db, referenceTime, null);
        }

        /// <summary>
        /// Generates notifications for new Medical Appointments
        /// </summary>
        private static void GenerateNotificationsForNewGenericAppointments(
            [NotNull] CerebelloEntities db, DateTime referenceTime, [NotNull] ICollection<Tuple<Notification, object>> notificationsToBeDispatched)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (notificationsToBeDispatched == null) throw new ArgumentNullException("notificationsToBeDispatched");

            // check for appointments that have to be notified
            var timeOffset = referenceTime.AddMinutes(30);
            var unnotifiedAppointments =
                db.Appointments.Where(
                    a => !a.Notified && a.Type == (int)TypeAppointment.GenericAppointment && a.Start >= referenceTime && a.Start < timeOffset).ToList();
            foreach (var appointment in unnotifiedAppointments)
            {
                Debug.Assert(appointment.PatientId != null, "appointment.PatientId != null");

                var genericAppointmentData = new GenericAppointmentNotificationData()
                {
                    Text = appointment.Description,
                    Time = DateTimeHelper.GetFormattedTime(
                        PracticeController.ConvertToLocalDateTime(appointment.Practice, appointment.Start))
                };

                var genericAppointmentDataString = new JavaScriptSerializer().Serialize(genericAppointmentData);

                // notify the doctor
                var newNotification = new Notification()
                {
                    CreatedOn = referenceTime,
                    IsClosed = false,
                    UserToId = appointment.DoctorId,
                    Type = NotificationConstants.GENERIC_APPOINTMENT_NOTIFICATION_TYPE,
                    PracticeId = appointment.PracticeId,
                    Data = genericAppointmentDataString
                };

                appointment.Doctor.Users.First().Notifications.Add(newNotification);
                notificationsToBeDispatched.Add(new Tuple<Notification, object>(newNotification, genericAppointmentData));

                appointment.Notified = true;
            }
            db.SaveChanges();
        }


    }
}