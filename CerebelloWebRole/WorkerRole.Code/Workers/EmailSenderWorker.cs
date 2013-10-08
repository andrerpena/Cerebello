using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Helpers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    /// <summary>
    /// Sends e-mails to patients.
    /// </summary>
    public class EmailSenderWorker : BaseCerebelloWorker
    {
        private static int locker;

        /// <summary>
        /// Gets the number of e-mails sent.
        /// </summary>
        public int EmailsCount { get; private set; }

        /// <summary>
        /// Runs the worker once to send e-mails.
        /// </summary>
        public override void RunOnce()
        {
            // If this method is already running, then leave the already running alone, and return.
            // If it is not running, set the value os locker to 1 to indicate that now it is running.
            if (Interlocked.Exchange(ref locker, 1) != 0)
                return;

            Trace.TraceInformation("EmailSenderWorker.RunOnce(): service in execution");
            var emailsSent = 0;

            try
            {
                var utcNow = this.GetUtcNow();
                using (var db = this.CreateNewCerebelloEntities())
                {
                    Trace.TraceInformation("EmailSenderWorker.RunOnce(): this.CreateNewCerebelloEntities()");

                    var next40H = utcNow.AddHours(40.0);

                    // One e-mail will be sent 40 hours before the appointment.
                    var items = db.Appointments
                        .Where(a => a.Start > utcNow && a.Start < next40H && !a.ReminderEmailSent)
                        .Where(a => a.PatientId != null)
                        .Select(
                            a => new
                                {
                                    Appointment = a,
                                    Practice = db.Practices.FirstOrDefault(p => p.Id == a.PracticeId),
                                    a.Patient,
                                    a.Patient.Person,
                                    a.Doctor,
                                    DoctorPerson = a.Doctor.Users.FirstOrDefault().Person,
                                })
                        .Select(
                            a => new
                                {
                                    a.Appointment,
                                    EmailViewModel = new PatientEmailModel
                                        {
                                            PatientEmail = a.Person.Email,
                                            PatientFirstName = a.Person.FirstName,
                                            PatientLastName = a.Person.LastName,
                                            PracticeUrlId = a.Practice.UrlIdentifier,
                                            PatientGender = (TypeGender)a.Person.Gender,
                                            PracticeName = a.Practice.Name,
                                            DoctorFirstName = a.DoctorPerson.FirstName,
                                            DoctorLastName = a.DoctorPerson.LastName,
                                            DoctorGender = (TypeGender)a.DoctorPerson.Gender,
                                            AppointmentDate = a.Appointment.Start,

                                            PracticeEmail = a.Practice.Email,
                                            PracticePhoneMain = a.Practice.PhoneMain,
                                            PracticePhoneAlt = a.Practice.PhoneAlt,
                                            PracticeSiteUrl = a.Practice.SiteUrl,
                                            PracticePabx = a.Practice.PABX,
                                            PracticeAddress = new AddressViewModel
                                                {
                                                    AddressLine1 = a.Practice.Address.AddressLine1,
                                                    AddressLine2 = a.Practice.Address.AddressLine2,
                                                    City = a.Practice.Address.City,
                                                    County = a.Practice.Address.County,
                                                    StateProvince = a.Practice.Address.StateProvince,
                                                    ZipCode = a.Practice.Address.ZipCode
                                                },

                                            // todo: is it correct to send these informations to the patient
                                            // maybe this info is not suited to the outside world
                                            DoctorPhone = a.DoctorPerson.PhoneMobile ?? a.DoctorPerson.PhoneWork,
                                            DoctorEmail = a.DoctorPerson.Email,
                                        }
                                })
                        .ToArray();

                    Trace.TraceInformation("EmailSenderWorker.RunOnce(): got list of appointments from database");

                    bool traceMessageCreated = false;
                    bool traceEmailSent = false;
                    bool traceEmailNotSent = false;
                    bool traceDbSaved = false;
                    foreach (var eachItem in items)
                    {
                        if (string.IsNullOrWhiteSpace(eachItem.EmailViewModel.PatientEmail))
                            continue;

                        // Rendering message bodies from partial view.
                        var emailViewModel = eachItem.EmailViewModel;
                        var toAddress = new MailAddress(emailViewModel.PatientEmail, PersonHelper.GetFullName(emailViewModel.PatientFirstName, null, emailViewModel.PatientLastName));
                        var mailMessage = this.CreateEmailMessage("AppointmentReminderEmail", toAddress, emailViewModel);

                        if (!traceMessageCreated)
                            Trace.TraceInformation("EmailSenderWorker.RunOnce(): var mailMessage = { rendered e-mail message }");
                        traceMessageCreated = true;

                        if (this.TrySendEmail(mailMessage))
                        {
                            if (!traceEmailSent)
                                Trace.TraceInformation("EmailSenderWorker.RunOnce(): this.TrySendEmail(mailMessage) => true");
                            traceEmailSent = true;

                            emailsSent++;
                            this.EmailsCount++;
                            eachItem.Appointment.ReminderEmailSent = true;
                            db.SaveChanges();

                            if (!traceDbSaved)
                                Trace.TraceInformation("EmailSenderWorker.RunOnce(): db.SaveChanges()");
                            traceDbSaved = true;
                        }
                        else
                        {
                            if (!traceEmailNotSent)
                                Trace.TraceInformation("EmailSenderWorker.RunOnce(): this.TrySendEmail(mailMessage) => false");
                            traceEmailNotSent = true;
                        }
                    }

                    Trace.TraceInformation(string.Format("EmailSenderWorker.RunOnce(): emailsSent => {0}", emailsSent));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("EmailSenderWorker.RunOnce(): exception while trying to send e-mails: {0}", TraceHelper.GetExceptionMessage(ex)));
            }

            // setting locker value to 0
            if (Interlocked.Exchange(ref locker, 0) != 1)
                throw new Exception("The value of locker should be 1 before setting it to 0.");
        }

    }
}
