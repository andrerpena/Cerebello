using System.Linq;
using System.Net.Mail;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    public class EmailSenderWorker : BaseCerebelloWorker
    {
        public static readonly object locker = new object();

        public override void RunOnce()
        {
            lock (locker)
            {
                var utcNow = this.GetUtcNow();
                using (var db = this.CreateNewCerebelloEntities())
                {
                    var next40H = utcNow.AddHours(40.0);
                    // One e-mail will be sent 40 hours before the appointment.
                    var items = db.Appointments
                        .Where(a => a.Start > utcNow && a.Start < next40H && !a.ReminderEmailSent)
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
                        if (!string.IsNullOrWhiteSpace(eachItem.EmailViewModel.PatientEmail))
                        {
                            // Rendering message bodies from partial view.
                            var emailViewModel = eachItem.EmailViewModel;

                            var bodyText = this.RenderView("AppointmentReminderEmail", emailViewModel);

                            emailViewModel.IsBodyHtml = true;
                            var bodyHtml = this.RenderView("AppointmentReminderEmail", emailViewModel);

                            var toAddress = new MailAddress(
                                eachItem.EmailViewModel.PatientEmail,
                                eachItem.EmailViewModel.PatientName);

                            var message = EmailHelper.CreateEmailMessage(
                                toAddress,
                                string.Format("Confirmação de consulta - {0}", eachItem.EmailViewModel.PracticeName),
                                bodyHtml,
                                bodyText);

                            try
                            {
                                this.SendEmail(message);

                                eachItem.Appointment.ReminderEmailSent = true;

                                db.SaveChanges();
                            }
                            catch
                            {
                                // Just ignore any errors... if e-mail was not sent,
                                // it is not marked with ReminderEmailSent = true,
                                // and so, it will just be sent later.
                            }
                        }
                    }
                }
            }
        }

    }
}
