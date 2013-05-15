using System;
using System.Web;
using System.Web.Mvc;

namespace CerebelloWebRole.Controllers
{
    public class GoogleDriveCallbackController : Controller
    {
        public ActionResult AssociateGoogleDrive(string state, string code)
        {
            var googleSessionCookie = this.Request.Cookies["google-drive-session-token"];
            if (googleSessionCookie == null || googleSessionCookie.Value == null)
                throw new Exception("Could not find verification session cookie");

            var googleSessionStateValues = state.Split('|');
            if (googleSessionStateValues.Length != 2)
                throw new Exception("Could not validate Google state");

            if (googleSessionStateValues[0] != googleSessionCookie.Value)
                throw new Exception("Could not validate request");

            var associateGoogleDriveUriBuilder = new UriBuilder(HttpUtility.UrlDecode(googleSessionStateValues[1]));

            var queryParameters = HttpUtility.ParseQueryString(associateGoogleDriveUriBuilder.Query);
            queryParameters.Set("code", code);

            associateGoogleDriveUriBuilder.Query = queryParameters.ToString();

            return this.Redirect(associateGoogleDriveUriBuilder.Uri.ToString());
        }
    }
}