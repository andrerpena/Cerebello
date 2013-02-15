using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace CerebelloWebRole.Code.Helpers
{
    public class EmailHelper
    {
        /// <summary>
        /// Creates an SmtpClient that will be used to send e-mails.
        /// </summary>
        /// <returns></returns>
        public static SmtpClient CreateSmtpClient()
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("cerebello@cerebello.com.br", "26uj27oP")
            };

            return smtpClient;
        }

        /// <summary>
        /// Creates an e-mail message.
        /// The 'From' address is fixed, and is valid in the Smtp server used by the 'SendEmail' method.
        /// </summary>
        /// <param name="toAddress">Address to send the message to.</param>
        /// <param name="subject">Subject of the message.</param>
        /// <param name="bodyText">Body of the message in plain text format.</param>
        /// <param name="bodyHtml">Body of the message in Html format.</param>
        /// <param name="sourceName"></param>
        /// <returns>Returns a 'MailMessage' that can be sent using the 'SendEmail' method.</returns>
        public static MailMessage CreateEmailMessage(
            MailAddress toAddress,
            [Localizable(true)] string subject,
            [Localizable(true)] string bodyText,
            [Localizable(true)] string bodyHtml = null,
            string sourceName = null)
        {
            // note: this method was copied to EmailSenderWorker
            if (string.IsNullOrEmpty(bodyText))
                throw new ArgumentException("bodyText must be provided.", "bodyText");

            if (Configuration.Instance.EmailAddressOverride)
                toAddress = new MailAddress("cerebello@cerebello.com.br", toAddress.DisplayName);

            // NOTE: The string "cerebello@cerebello.com.br" is repeated in other place.
            var fromAddress = new MailAddress("cerebello@cerebello.com.br", sourceName ?? "www.cerebello.com.br");
            var mailMessage = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = bodyText.Trim() };

            // Adding Html body.
            if (!string.IsNullOrWhiteSpace(bodyHtml))
                mailMessage.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(bodyHtml.Trim(), new ContentType(MediaTypeNames.Text.Html)));

            return mailMessage;
        }

        /// <summary>
        /// Delegate that represents a method that can send an email.
        /// </summary>
        /// <param name="mailMessage"></param>
        public delegate void SendEmailAction(MailMessage mailMessage);

        /// <summary>
        /// Overrides the default procedure DefaultSendEmail when calling SendEmail.
        /// </summary>
        public static SendEmailAction DefaultEmailSender { get; set; }

        /// <summary>
        /// Represents the default e-mail sending procedure.
        /// This method may not be used directly, use SendEmail instead.
        /// </summary>
        /// <param name="mailMessage"></param>
        public static void DefaultSendEmail(MailMessage mailMessage)
        {
            using (var smtpClient = CreateSmtpClient())
                smtpClient.Send(mailMessage);
        }

        /// <summary>
        /// Sends an e-mail message using the default SmtpClient.
        /// The e-mail will be sent by either calling the DefaultSendEmailAction delegate or the DefaultSendEmail method.
        /// </summary>
        /// <param name="mailMessage">The MailMessage to send.</param>
        public static void SendEmail(MailMessage mailMessage)
        {
#if DEBUG
            var allowSendEmail = mailMessage.To.All(to => new[]
                {
                    // These are the allowed email destinations when debugging.
                    // If the e-mail address is not in this list, then it will be saved locally.
                    "masbicudo@gmail.com",
                    "andrerpena@gmail.com",
                    "cerebello@cerebello.com.br",
                }.Contains(to.Address));
#else
            var allowSendEmail = true;
#endif

            if (!allowSendEmail || Configuration.Instance.UseDesktopEmailBox)
            {
                SaveEmailLocal(mailMessage);
            }
            else
            {
                (DefaultEmailSender ?? DefaultSendEmail)(mailMessage);
            }
        }

        private static void SaveEmailLocal(MailMessage message)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var emailsPath = Path.Combine(desktopPath, @"Emails");

            foreach (var eachDestinationAddress in message.To)
            {
                var inboxPath = Path.Combine(emailsPath,
                    string.Format("{0}@{1}",
                        Regex.Replace(eachDestinationAddress.User, @"(?:[^\w\d]|[\s\.])+", ".", RegexOptions.IgnoreCase),
                        Regex.Replace(eachDestinationAddress.Host, @"(?:[^\w\d]|[\s\.])+", ".", RegexOptions.IgnoreCase)));

                if (!Directory.Exists(emailsPath))
                    Directory.CreateDirectory(emailsPath);

                if (!Directory.Exists(inboxPath))
                    Directory.CreateDirectory(inboxPath);

                var name = message.Subject;
                name = Regex.Replace(name, @"(?:[^\w\d]|[\s\.])+", ".", RegexOptions.IgnoreCase).Trim('.');

                using (var file = System.IO.File.Create(Path.Combine(inboxPath, name + ".html")))
                {
                    message.AlternateViews.Single(x => x.ContentType.MediaType == "text/html").ContentStream.CopyTo(file);
                }

                using (var file = System.IO.File.Create(Path.Combine(inboxPath, name + ".txt")))
                {
                    message.AlternateViews.Single(x => x.ContentType.MediaType == "text/plain").ContentStream.CopyTo(file);
                }
            }
        }

    }
}