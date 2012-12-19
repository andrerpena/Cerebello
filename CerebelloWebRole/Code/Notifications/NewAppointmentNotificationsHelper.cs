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
        /// <param name="userId"> </param>
        /// <param name="utcNowGetter"></param>
        /// <param name="urlHelper"></param>
        /// <param name="practiceId"></param>
        /// <param name="polled"></param>
        /// <returns></returns>
        public static List<NewAppointmentNotificationData> GetNewAppointmentNotifications([NotNull] CerebelloEntitiesAccessFilterWrapper db, int practiceId, int userId, [NotNull] Func<DateTime> utcNowGetter, [NotNull] UrlHelper urlHelper, bool polled = false)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (utcNowGetter == null) throw new ArgumentNullException("utcNowGetter");
            if (urlHelper == null) throw new ArgumentNullException("urlHelper");

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
                       Where(a => a.Start >= timeThresholdMin && a.Start < timeThresholdMax && a.Status == (int)TypeAppointmentStatus.Undefined && a.IsPolled == polled).ToList();

                if (scheduledAppointments.Any())
                {
                    foreach (var appointment in scheduledAppointments)
                    {
                        var data = new NewAppointmentNotificationData
                            {
                                NotificationId = appointment.Id,
                                NotificationClientId = "appointment_" + appointment.Id,
                                AppointmentTime =
                                    DateTimeHelper.GetFormattedTime(
                                        PracticeController.ConvertToLocalDateTime(practice, appointment.Start)),
                                DoctorId = appointment.DoctorId,
                                DoctorName = appointment.Doctor.Users.First().Person.FullName,
                                DoctorUrl =
                                    urlHelper.Action(
                                        "Index",
                                        "DoctorHome",
                                        new { doctor = appointment.Doctor.UrlIdentifier }),
                                AppointmentAccomplishedUrl =
                                    urlHelper.Action(
                                        "SetAppointmentAsAccomplished",
                                        "Notifications",
                                        new { id = appointment.Id, createDoctorNotification = true }),
                                AppointmentCanceledUrl =
                                    urlHelper.Action(
                                        "SetAppointmentAsCanceled",
                                        "Notifications",
                                        new { id = appointment.Id }),
                                AppointmentIsPolledUrl =
                                    urlHelper.Action("SetAppointmentAsPolled", "Notifications", new { id = appointment.Id })
                            };

                        var patient = appointment.Patient;
                        if (patient != null)
                        {
                            data.PatientId = patient.Id;
                            data.PatientName = patient.Person.FullName;
                            data.PatientUrl = urlHelper.Action("Details", "Patients", new { id = patient.Id });

                        }
                        result.Add(data);
                    }
                }
            }

            return result;
        }
    }
}