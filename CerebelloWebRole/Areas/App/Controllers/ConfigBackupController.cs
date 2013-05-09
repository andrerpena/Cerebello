using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Google;
using CerebelloWebRole.Code.Google.Data;
using CerebelloWebRole.Code.Notifications;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [UserRolePermission(UserRoleFlags.Owner)]
    public class ConfigBackupController : DoctorController
    {
        //
        // GET: /App/Config/

        public ActionResult Index()
        {
            var viewModel = new ConfigBackupViewModel();
            var dbGoogleAccountInfo = this.db.GoogleUserAccountInfo.FirstOrDefault();

            if (dbGoogleAccountInfo != null)
            {
                viewModel.GoogleDriveAssociated = true;
                viewModel.GoogleDriveEmail = dbGoogleAccountInfo.Email;
            }
            else
            {
                viewModel.GoogleDriveAssociated = false;

                // this url needs to be absolute, because it's going to Google
                var returnUrl = string.Format(
                    "{0}://{1}{2}",
                    this.Url.RequestContext.HttpContext.Request.Url.Scheme,
                    this.Url.RequestContext.HttpContext.Request.Url.Host,
                    this.Url.Action("AssociateGoogleDrive", "GoogleDriveCallback", new { area = (string)null }));


                // this url can be relative
                var associateUrl = string.Format(
                    "{0}://{1}{2}",
                    this.Url.RequestContext.HttpContext.Request.Url.Scheme,
                    this.Url.RequestContext.HttpContext.Request.Url.Host,
                    this.Url.Action("AssociateGoogleDrive"));

                var googleSessionToken = Guid.NewGuid().ToString();
                this.Response.Cookies.Add(new HttpCookie("google-drive-session-token", googleSessionToken));

                var authorizationUrlBuilder = new UriBuilder("https://accounts.google.com/o/oauth2/auth");
                var queryParameters = HttpUtility.ParseQueryString(authorizationUrlBuilder.Query);

                queryParameters.Set("client_id", "647667148.apps.googleusercontent.com");
                queryParameters.Set("response_type", "code");
                queryParameters.Set("approval_prompt", "force");
                queryParameters.Set("access_type", "offline");
                queryParameters.Set(
                    "scope",
                    "https://www.googleapis.com/auth/drive https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile");
                queryParameters.Set("redirect_uri", returnUrl);
                queryParameters.Set("state", googleSessionToken + "|" + HttpUtility.UrlEncode(associateUrl));

                authorizationUrlBuilder.Query = queryParameters.ToString();

                viewModel.GoogleDriveAuthorizationUrl = authorizationUrlBuilder.Uri.ToString();
            }

            return this.View(viewModel);
        }

        /// <summary>
        /// Associates a Google Drive account
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="oauth_token"></param>
        /// <returns></returns>
        public ActionResult AssociateGoogleDrive(string code)
        {
            var authorizationState = GoogleApiHelper.ExchangeCode(code);

            var client = new WebClient();
            client.Headers["Authorization"] = "OAuth " + authorizationState.AccessToken;
            var resultBytes = client.DownloadData("https://www.googleapis.com/oauth2/v2/userinfo");
            var resultString = Encoding.UTF8.GetString(resultBytes);
            var result = new JavaScriptSerializer().Deserialize<AccountInfoJsonResult>(resultString);

            this.db.GoogleUserAccountInfo.AddObject(
                new GoogleUserAccoutInfo()
                    {
                        AuthenticationCode = code,
                        Email = result.email,
                        Name = result.name,
                        PracticeId = this.DbPractice.Id,
                        PersonId = this.DbUser.Id,
                        RefreshToken = authorizationState.RefreshToken
                    });

            this.db.Notifications.AddObject(new Notification
                {
                    CreatedOn = this.GetUtcNow(),
                    PracticeId = this.DbPractice.Id,
                    UserToId = this.DbUser.Id,
                    Type = NotificationConstants.GOOGLE_DRIVE_ASSOCIATED_NOTIFICATION_TYPE
                });

            this.db.SaveChanges();

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Desassociates a Google Drive account
        /// </summary>
        /// <returns></returns>
        public ActionResult DesassociateGoogleDrive()
        {
            var dbGoogleAccountInfo = this.db.GoogleUserAccountInfo.FirstOrDefault();
            if (dbGoogleAccountInfo != null)
                this.db.GoogleUserAccountInfo.DeleteObject(dbGoogleAccountInfo);

            var dbNotification = new Notification
                {
                    CreatedOn = this.GetUtcNow(),
                    PracticeId = this.DbPractice.Id,
                    UserToId = this.DbUser.Id,
                    Type = NotificationConstants.GOOGLE_DRIVE_DESASSOCIATED_NOTIFICATION_TYPE
                };

            this.db.Notifications.AddObject(dbNotification);

            // mark all patients as not backed up
            foreach (var patient in this.db.Patients)
                patient.IsBackedUp = false;

            this.db.SaveChanges();

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Marks all patients to backup
        /// </summary>
        /// <returns></returns>
        public ActionResult MarkAllToBackup()
        {
            foreach (var patient in this.db.Patients)
                patient.IsBackedUp = false;
            this.db.SaveChanges();
            return null;
        }
    }
}