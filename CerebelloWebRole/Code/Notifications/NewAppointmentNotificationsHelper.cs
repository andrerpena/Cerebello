using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Notifications
{
    public static class NewAppointmentNotificationsHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="utcNowGetter"></param>
        /// <param name="url"></param>
        /// <param name="practiceId"></param>
        /// <param name="polled"></param>
        /// <returns></returns>
        public static List<NewAppointmentNotificationData> GetNewAppointmentNotifications([NotNull] CerebelloEntitiesAccessFilterWrapper db, int practiceId, int userId, [NotNull] Func<DateTime> utcNowGetter, [NotNull] UrlHelper url, bool polled = false)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (utcNowGetter == null) throw new ArgumentNullException("utcNowGetter");
            if (url == null) throw new ArgumentNullException("url");

            var practice = db.Practices.First(p => p.Id == practiceId);
            var user = db.Users.First(u => u.Id == userId);

            var result = new List<NewAppointmentNotificationData>();

            if (user.Secretary != null)
            {
                var utcNow = utcNowGetter();
                var timeThresholdMin = utcNow.AddMinutes(-30);
                var timeThresholdMax = utcNow.AddMinutes(10);

                // in this case this user is a secretary
                var scheduledAppointments =
                    db.Appointments.Include("Doctor").Include("Patient").
                       Where(a => a.Start >= timeThresholdMin && a.Start < timeThresholdMax && a.Status == AppointmentStatus.Undefined && a.IsPolled == polled).ToList();

                if (scheduledAppointments.Any())
                {
                    foreach (var appointment in scheduledAppointments)
                        result.Add(new NewAppointmentNotificationData()
                        {
                            NotificationId = appointment.Id,
                            NotificationClientId = "appointment_" + appointment.Id,
                            AppointmentTime =
                                DateTimeHelper.GetFormattedTime(
                                    PracticeController.ConvertToLocalDateTime(practice, appointment.Start)),
                            DoctorId = appointment.DoctorId,
                            DoctorName = appointment.Doctor.Users.First().Person.FullName,
                            DoctorUrl =
                                url.Action("Index", "DoctorHome",
                                           new { doctor = appointment.Doctor.UrlIdentifier }),
                            PatientId = appointment.Patient.Id,
                            PatientName = appointment.Patient.Person.FullName,
                            PatientUrl =
                                url.Action("Details", "Patients", new { id = appointment.Patient.Id }),
                            AppointmentAccomplishedUrl =
                                url.Action("SetAppointmentAsAccomplished", "Notifications",
                                           new { id = appointment.Id, createDoctorNotification = true }),
                            AppointmentCanceledUrl =
                                url.Action("SetAppointmentAsCanceled", "Notifications",
                                           new { id = appointment.Id }),
                            AppointmentIsPolledUrl = url.Action("SetAppointmentAsPolled", "Notifications", new { id = appointment.Id })

                        });
                }
            }

            return result;
        }
    }
}