using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;

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

            Trace.TraceInformation("E-mail notification service in execution (EmailSenderWorker)");

            try
            {
                var utcNow = this.GetUtcNow();
                using (var db = this.CreateNewCerebelloEntities())
                {
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
                                            PatientName = a.Person.FullName,
                                            PracticeUrlId = a.Practice.UrlIdentifier,
                                            PatientGender = (TypeGender)a.Person.Gender,
                                            PracticeName = a.Practice.Name,
                                            DoctorName = a.DoctorPerson.FullName,
                                            DoctorGender = (TypeGender)a.DoctorPerson.Gender,
                                            AppointmentDate = a.Appointment.Start,

                                            PracticeEmail = a.Practice.Email,
                                            PracticePhoneMain = a.Practice.PhoneMain,
                                            PracticePhoneAlt = a.Practice.PhoneAlt,
                                            PracticeSiteUrl = a.Practice.SiteUrl,
                                            PracticePabx = a.Practice.PABX,
                                            PracticeAddress = new AddressViewModel
                                                {
                                                    StateProvince = a.Practice.Address.StateProvince,
                                                    City = a.Practice.Address.City,
                                                    Neighborhood = a.Practice.Address.Neighborhood,
                                                    Street = a.Practice.Address.Street,
                                                    Complement = a.Practice.Address.Complement,
                                                    CEP = a.Practice.Address.CEP,
                                                },

                                            // todo: is it correct to send these informations to the patient
                                            // maybe this info is not suited to the outside world
                                            DoctorPhone = a.DoctorPerson.PhoneCell ?? a.DoctorPerson.PhoneLand,
                                            DoctorEmail = a.DoctorPerson.Email,
                                        }
                                })
                        .ToArray();

                    foreach (var eachItem in items)
                    {
                        if (string.IsNullOrWhiteSpace(eachItem.EmailViewModel.PatientEmail))
                            continue;

                        // Rendering message bodies from partial view.
                        var emailViewModel = eachItem.EmailViewModel;
                        var toAddress = new MailAddress(emailViewModel.PatientEmail, emailViewModel.PatientName);
                        var mailMessage = this.CreateEmailMessage("AppointmentReminderEmail", toAddress, emailViewModel);

                        if (this.TrySendEmail(mailMessage))
                        {
                            this.EmailsCount++;
                            eachItem.Appointment.ReminderEmailSent = true;
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error sending e-mails. Ex: " + ex.Message);
            }

            // setting locker value to 0
            if (Interlocked.Exchange(ref locker, 0) != 1)
                throw new Exception("The value of locker should be 1 before setting it to 0.");
        }

    }
}
