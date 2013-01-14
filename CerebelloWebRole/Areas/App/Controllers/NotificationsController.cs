using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class NotificationsController : PracticeController
    {
        /// <summary>
        /// Sets the appointment as polled. The appointments cannot be set as polled by the time
        /// they are sent to the client because there is a possibility that the appointment
        /// was polled by a "dead thread", that is, a thread that was running but is never going
        /// to make it to the client.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult SetAppointmentAsPolled(int id)
        {
            try
            {
                var appointment = this.db.Appointments.First(a => a.Id == id);
                appointment.IsPolled = true;
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                this.Response.StatusCode = (int)(HttpStatusCode.InternalServerError);
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Sets the appointment status to accomplished
        /// </summary>
        /// <param name="id"></param>
        /// <param name="createDoctorNotification"></param>
        /// <returns></returns>
        public JsonResult SetAppointmentAsAccomplished(int id, bool createDoctorNotification)
        {
            try
            {
                var appointment = this.db.Appointments.First(a => a.Id == id);
                appointment.Status = (int) TypeAppointmentStatus.Accomplished;

                if (createDoctorNotification)
                {
                    var notificationText = string.Format("<a href='{0}'>{1}</a> chegou para uma consulta às {2}",
                        this.Url.Action("Details", "Patients", new { id = appointment.PatientId }),
                        appointment.Patient.Person.FullName,
                        DateTimeHelper.GetFormattedTime(PracticeController.ConvertToLocalDateTime(this.DbPractice, appointment.Start)));

                    var notification = new Notification()
                        {
                            CreatedOn = this.GetUtcNow(),
                            UserId = appointment.DoctorId,
                            PracticeId = appointment.PracticeId,
                            Text = notificationText
                        };

                    this.db.Notifications.AddObject(notification);
                }

                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                this.Response.StatusCode = (int)(HttpStatusCode.InternalServerError);
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Sets the appointment status to accomplished
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult SetAppointmentAsCanceled(int id)
        {
            try
            {
                var appointment = this.db.Appointments.First(a => a.Id == id);
                appointment.Status = (int)TypeAppointmentStatus.NotAccomplished;
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                this.Response.StatusCode = (int)(HttpStatusCode.InternalServerError);
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Sets the appointment status to discarded
        /// </summary>
        /// <remarks>
        /// This is only for generic appointments
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult SetAppointmentAsDiscarded(int id)
        {
            try
            {
                var appointment = this.db.Appointments.First(a => a.Id == id);
                appointment.Status = (int)TypeAppointmentStatus.Discarded;
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                this.Response.StatusCode = (int)(HttpStatusCode.InternalServerError);
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Sets the notification as polled. The notifications cannot be set as polled by the time
        /// they are sent to the client because there is a possibility that the notification
        /// was polled by a "dead thread", that is, a thread that was running but is never going
        /// to make it to the client.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SetNotificationAsPolled(int id)
        {
            try
            {
                var notification = this.db.Notifications.First(a => a.Id == id);
                notification.IsPolled = true;
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                this.Response.StatusCode = (int)(HttpStatusCode.InternalServerError);
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Removes the notification
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RemoveNotification(int id)
        {
            try
            {
                var notification = this.db.Notifications.First(a => a.Id == id);
                this.db.Notifications.DeleteObject(notification);
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                this.Response.StatusCode = (int)(HttpStatusCode.InternalServerError);
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}