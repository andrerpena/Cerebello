using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.Helpers
{
    /// <summary>
    /// Helps with creating and sending e-mails.
    /// </summary>
    public class EmailHelper
    {
        /// <summary>
        /// Creates an SmtpClient that will be used to send e-mails.
        /// </summary>
        /// <returns>Returns the default Smtp client to be used in Cerebello.</returns>
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

        public const string DEFAULT_SOURCE = "www.cerebello.com.br";

        /// <summary>
        /// Creates an e-mail message.
        /// The 'From' address is fixed, and is valid in the Smtp server used by the 'TrySendEmail' method.
        /// </summary>
        /// <param name="toAddress">Address to send the message to.</param>
        /// <param name="subject">Subject of the message.</param>
        /// <param name="bodyText">Body of the message in plain text format.</param>
        /// <param name="bodyHtml">Body of the message in Html format.</param>
        /// <param name="sourceName">Source name of the e-mail.</param>
        /// <returns>Returns a 'MailMessage' that can be sent using the 'TrySendEmail' method.</returns>
        public static MailMessage CreateEmailMessage(
            MailAddress toAddress,
            [Localizable(true)] string subject,
            [Localizable(true)] string bodyText,
            [Localizable(true)] string bodyHtml = null,
            string sourceName = DEFAULT_SOURCE)
        {
            // note: this method was copied to EmailSenderWorker
            if (string.IsNullOrEmpty(bodyText))
                throw new ArgumentException("bodyText must not be null nor empty.", "bodyText");

            // NOTE: The string "cerebello@cerebello.com.br" is repeated in other place.
            var fromAddress = new MailAddress("cerebello@cerebello.com.br", sourceName ?? DEFAULT_SOURCE);
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
        /// Overrides the default procedure DefaultSendEmail when calling TrySendEmail.
        /// </summary>
        public static SendEmailAction DefaultEmailSender { get; set; }

        /// <summary>
        /// Represents the default e-mail sending procedure.
        /// This method may not be used directly, use TrySendEmail instead.
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
            if (DebugConfig.IsDebug)
            {
                // saving e-mail to the file-system
                foreach (var storageEmailSavers in DebugConfig.StorageEmailSavers)
                    storageEmailSavers(mailMessage);

                foreach (var addressToSendTo in DebugConfig.EmailAddressesToCopyEmailsTo)
                    // ReSharper disable AccessToForEachVariableInClosure
                    foreach (var emailAddress in mailMessage.To.Select(x => new MailAddress(addressToSendTo, x.DisplayName)))
                        // ReSharper restore AccessToForEachVariableInClosure
                        mailMessage.Bcc.Add(emailAddress);

                // removing all unallowed email addresses
                var notAllowed = mailMessage.To.Where(a => !DebugConfig.CanSendEmailToAddress(a.Address)).ToList();
                foreach (var address in notAllowed)
                    mailMessage.To.Remove(address);

                if (notAllowed.Any() && !mailMessage.To.Any())
                {
                    if (!mailMessage.Bcc.Any())
                    {
                        // ReSharper disable EmptyGeneralCatchClause
                        try
                        {
                            Debug.Print(
                                "E-mail ignored: cannot send to the address ({0}) while in DEBUG mode.",
                                notAllowed.First().Address);
                        }
                        catch
                        {
                        }
                        // ReSharper restore EmptyGeneralCatchClause

                        return;
                    }

                    mailMessage.To.Add(mailMessage.Bcc[0]);
                    mailMessage.Bcc.RemoveAt(0);
                }

                // prepending "[TEST]" when in debug (this will help differentiate real messages from test messages)
                mailMessage.Subject = "[TEST] " + mailMessage.Subject;
            }
#endif

            if (!mailMessage.To.Any())
            {
                mailMessage.Subject = string.Format("WARNING: E-MAIL W/O DESTINATION: {0}", mailMessage.Subject);
                mailMessage.To.Add(new MailAddress("cerebello@cerebello.com.br", "Error report"));
            }

            (DefaultEmailSender ?? DefaultSendEmail)(mailMessage);
        }

        /// <summary>
        /// Tries to send an e-mail message using the default SmtpClient.
        /// The e-mail will be sent by either calling the DefaultSendEmailAction delegate or the DefaultSendEmail method.
        /// </summary>
        /// <param name="mailMessage">The MailMessage to send.</param>
        public static bool TrySendEmail(MailMessage mailMessage)
        {
            for (int tries = 0; tries < 3; tries++)
            {
                // sleeping for 10 seconds if not first try
                if (tries > 0)
                    Thread.Sleep(10000);

                try
                {
                    SendEmail(mailMessage);
                    return true;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                {
                }
                // ReSharper restore EmptyGeneralCatchClause
            }
            return false;
        }

        /// <summary>
        /// Tries to send an e-mail message using the default SmtpClient asynchronously.
        /// </summary>
        /// <param name="mailMessage">The MailMessage to send.</param>
        /// <returns>Returns a Task object containing information about the task.</returns>
        public static Task SendEmailAsync(MailMessage mailMessage)
        {
            var task = new Task(() =>
                {
                    Exception exceptions = null;
                    for (int tries = 0; tries < 6; tries++)
                    {
                        // sleeping for 10 seconds if not first try
                        if (tries > 0)
                            Thread.Sleep(10000);

                        try
                        {
                            SendEmail(mailMessage);
                            return;
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch (Exception ex)
                        {
                            exceptions = ex;
                        }
                        // ReSharper restore EmptyGeneralCatchClause
                    }

                    // rethrows the last exception
                    if (exceptions != null)
                        throw exceptions;
                });

            task.Start();
            return task;
        }

        /// <summary>
        /// Saves an e-mail locally.
        /// </summary>
        /// <param name="message">E-mail message to be saved.</param>
        /// <param name="path">Path where e-mails should be saved.</param>
        /// <remarks>E-mails are organized automatically by email address using subdirectories.</remarks>
        internal static void SaveEmailLocal(MailMessage message, string path)
        {
            foreach (var eachDestinationAddress in message.To)
            {
                var inboxPath = Path.Combine(
                    path,
                    string.Format(
                        "{0}@{1}",
                        Regex.Replace(eachDestinationAddress.User, @"(?:[^\w\d]|[\s\.])+", ".", RegexOptions.IgnoreCase),
                        Regex.Replace(eachDestinationAddress.Host, @"(?:[^\w\d]|[\s\.])+", ".", RegexOptions.IgnoreCase)));

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!Directory.Exists(inboxPath))
                    Directory.CreateDirectory(inboxPath);

                var name = message.Subject;
                name = Regex.Replace(name, @"(?:[^\w\d]|[\s\.])+", ".", RegexOptions.IgnoreCase).Trim('.');

                var dicMediaTypeExt = new Dictionary<string, string>
                    {
                        { "text/plain", ".txt" },
                        { "text/html", ".html" },
                    };

                // saving main view
                string ext = message.IsBodyHtml ? ".html" : ".txt";
                using (var file = File.Create(Path.Combine(inboxPath, name + ext)))
                using (var writer = new StreamWriter(file, message.BodyEncoding))
                    writer.Write(message.Body);

                // saving alternate views
                foreach (var eachAlternateView in message.AlternateViews)
                {
                    string ext2;
                    if (dicMediaTypeExt.TryGetValue(eachAlternateView.ContentType.MediaType, out ext2))
                        using (var file = File.Create(Path.Combine(inboxPath, name + ext2)))
                        {
                            eachAlternateView.ContentStream.CopyTo(file);
                            eachAlternateView.ContentStream.Position = 0;
                        }
                }
            }
        }

        /// <summary>
        /// Creates an e-mail message from an MVC view.
        /// The title, the html and text contents of the e-mail will be given by this view.
        /// The 'From' address is fixed, and is valid in the Smtp server used by the 'TrySendEmail' method.
        /// </summary>
        /// <param name="viewRenderer">Method that is used to render the view.</param>
        /// <param name="contentView">View name to use to render the e-mail contents, and to get the text from.</param>
        /// <param name="toAddress">Address of the recipient.</param>
        /// <param name="model">Data that should be sent to the view.</param>
        /// <param name="sourceName">Source name for this e-mail.</param>
        /// <returns>Returns a 'MailMessage' that can be sent using the 'TrySendEmail' method.</returns>
        public static MailMessage CreateEmailMessageFromView(
            Func<string, ViewDataDictionary, string> viewRenderer,
            [AspMvcView][AspMvcPartialView] string contentView,
            MailAddress toAddress,
            object model,
            string sourceName)
        {
            var viewData = new ViewDataDictionary(model);

            // generating html content
            viewData["IsBodyHtml"] = false;
            var bodyText = WebUtility.HtmlDecode(viewRenderer(contentView, viewData));

            // todo: one day if needed, we could use HtmlAgilityPack to read elements that need embeded resources and attatch them

            // generating text content
            viewData["IsBodyHtml"] = true;
            var bodyHtml = viewRenderer(contentView, viewData);

            // title is defined inside the view
            string undefinedTitle = "Sem título";
            if (DebugConfig.IsDebug)
                undefinedTitle = string.Format("DEBUG WARNING: 'ViewBag.Title' was not defined in view '{0}'", contentView);
            var title = (viewData["Title"] ?? undefinedTitle).ToString();

            var result = CreateEmailMessage(toAddress, title, bodyText, bodyHtml, sourceName);
            return result;
        }
    }
}