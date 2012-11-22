using System.Linq;
using System.Net.Mail;
using CerebelloWebRole.Code.Helpers;

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
                        .Select(a => new
                            {
                                Appointment = a,
                                a.Patient.Person,
                                Practice = db.Practices.FirstOrDefault(p => p.Id == a.PracticeId),
                            })
                        .ToArray();

                    foreach (var eachItem in items)
                    {
                        // Rendering message bodies from partial view.
                        var emailViewModel = new PatientEmailModel
                        {
                            PatientName = eachItem.Person.FullName,
                            PracticeUrlId = eachItem.Practice.UrlIdentifier,
                        };
                        var bodyText = this.RenderView("AppointmentReminderEmail", emailViewModel);

                        emailViewModel.IsBodyHtml = true;
                        var bodyHtml = this.RenderView("AppointmentReminderEmail", emailViewModel);

                        var toAddress = new MailAddress(eachItem.Person.Email, eachItem.Person.FullName);

                        var message = EmailHelper.CreateEmailMessage(
                            toAddress,
                            "Bem vindo ao Cerebello! Por favor, confirme a criação de sua conta.",
                            bodyHtml,
                            bodyText);

                        eachItem.Appointment.ReminderEmailSent = true;

                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
