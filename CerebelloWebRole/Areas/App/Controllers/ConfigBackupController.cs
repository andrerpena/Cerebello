using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Notifications;
using DropNet;
using DropNet.Models;

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
            var dbDropboxInfo = this.db.DropboxInfos.FirstOrDefault();

            if (dbDropboxInfo != null)
            {
                viewModel.DropboxAssociated = true;
                viewModel.DropboxEmail = dbDropboxInfo.Email;
            }
            else
            {
                viewModel.DropboxAssociated = false;

                var client = new DropNetClient("r1ndpw0o5lh755x", "qrmdxee9kzbd81i");
                var token = client.GetToken();

                this.Response.Cookies.Add(new HttpCookie("dropbox_user_token", new JavaScriptSerializer().Serialize(token)));

                var requestUrl = this.Url.RequestContext.HttpContext.Request.Url;

                Debug.Assert(requestUrl != null, "requestUrl != null");
                var absoluteAction = string.Format("{0}://{1}{2}",
                                                      requestUrl.Scheme,
                                                      requestUrl.Host,
                                                      this.Url.Action("AssociateDropbox", "ConfigBackup"));

                viewModel.DropboxAuthorizationUrl = client.BuildAuthorizeUrl(absoluteAction);
            }



            return this.View(viewModel);
        }

        /// <summary>
        /// Associates a Dropbox account
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="oauth_token"></param>
        /// <returns></returns>
        public ActionResult AssociateDropbox(string uid, string oauth_token)
        {
            var dropboxCookie = this.Request.Cookies["dropbox_user_token"];
            if (dropboxCookie == null)
                return this.RedirectToAction("Index");

            var userToken = new JavaScriptSerializer().Deserialize<UserLogin>(dropboxCookie.Value);
            var client = new DropNetClient("r1ndpw0o5lh755x", "qrmdxee9kzbd81i", userToken.Token, userToken.Secret);

            // this access token grants long during access permission
            var accessToken = client.GetAccessToken();
            var accountInfo = client.AccountInfo();

            var dbDropboxInfo = this.db.DropboxInfos.FirstOrDefault();
            if (dbDropboxInfo == null)
            {
                dbDropboxInfo = new DropboxInfo { PracticeId = this.DbPractice.Id };
                this.db.DropboxInfos.AddObject(dbDropboxInfo);
            }

            dbDropboxInfo.DisplayName = accountInfo.display_name;
            dbDropboxInfo.Email = accountInfo.email;
            dbDropboxInfo.Uid = accountInfo.uid;
            dbDropboxInfo.Token = accessToken.Token;
            dbDropboxInfo.Secret = accessToken.Secret;

            var dbNotification = new Notification
                {
                    CreatedOn = this.GetUtcNow(),
                    PracticeId = this.DbPractice.Id,
                    UserToId = this.DbUser.Id,
                    Type = NotificationConstants.DROPBOX_ASSOCIATED_NOTIFICATION_TYPE
                };

            this.db.Notifications.AddObject(dbNotification);
            this.db.SaveChanges();

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Desassociates a Dropbox account
        /// </summary>
        /// <returns></returns>
        public ActionResult DesassociateDropbox()
        {
            var dbDropboxInfo = this.db.DropboxInfos.FirstOrDefault();
            if (dbDropboxInfo != null)
                this.db.DropboxInfos.DeleteObject(dbDropboxInfo);

            var dbNotification = new Notification
            {
                CreatedOn = this.GetUtcNow(),
                PracticeId = this.DbPractice.Id,
                UserToId = this.DbUser.Id,
                Type = NotificationConstants.DROPBOX_DESASSOCIATED_NOTIFICATION_TYPE
            };

            this.db.Notifications.AddObject(dbNotification);

            // mark all patients as not backed up
            foreach (var patient in this.db.Patients)
                patient.IsBackedUp = false;

            this.db.SaveChanges();

            return this.RedirectToAction("Index");
        }
    }
}