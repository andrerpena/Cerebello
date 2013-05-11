using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using CerebelloWebRole.Code.Google.Data;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using JetBrains.Annotations;
using File = Google.Apis.Drive.v2.Data.File;

namespace CerebelloWebRole.Code.Google
{
    public static class GoogleApiHelper
    {
        /// <summary>
        /// Extends the NativeApplicationClient class to allow setting of a custom IAuthorizationState.
        /// </summary>
        public class StoredStateClient : NativeApplicationClient
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StoredStateClient"/> class.
            /// </summary>
            /// <param name="authorizationServer">The token issuer.</param>
            /// <param name="clientIdentifier">The client identifier.</param>
            /// <param name="clientSecret">The client secret.</param>
            public StoredStateClient(AuthorizationServerDescription authorizationServer,
                String clientIdentifier,
                String clientSecret,
                IAuthorizationState state)
                : base(authorizationServer, clientIdentifier, clientSecret)
            {
                this.State = state;
            }

            public IAuthorizationState State { get; private set; }

            /// <summary>
            /// Returns the IAuthorizationState stored in the StoredStateClient instance.
            /// </summary>
            /// <param name="provider">OAuth2 client.</param>
            /// <returns>The stored authorization state.</returns>
            static public IAuthorizationState GetState(StoredStateClient provider)
            {
                return provider.State;
            }
        }


        /// <summary>
        /// Exchange an authorization code for OAuth 2.0 credentials.
        /// </summary>
        /// <param name="authorizationCode">Authorization code to exchange for OAuth 2.0 credentials.</param>
        /// <param name="refreshToken"></param>
        /// <param name="callbackUrl"></param>
        /// <returns>OAuth 2.0 credentials.</returns>
        public static IAuthorizationState ExchangeCode([NotNull] String authorizationCode, string refreshToken,
                                                       [NotNull] string callbackUrl)
        {
            if (authorizationCode == null) throw new ArgumentNullException("authorizationCode");
            if (callbackUrl == null) throw new ArgumentNullException("callbackUrl");

            var provider = new NativeApplicationClient(
                GoogleAuthenticationServer.Description, "647667148.apps.googleusercontent.com", "SHvBqFmGtXq5bTPqY242oNvB");
            IAuthorizationState state = new AuthorizationState();
            state.Callback = new Uri(callbackUrl);
            state.RefreshToken = refreshToken;
            try
            {
                state = provider.ProcessUserAuthorization(authorizationCode, state);
                provider.RequestUserAuthorization();
                return state;
            }
            catch (ProtocolException)
            {
                throw new Exception(null);
            }
        }

        /// <summary>
        /// Returns an access token based on an refresh token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public static AccessTokenJsonResult RequestAccessToken([NotNull] string refreshToken)
        {
            if (refreshToken == null) throw new ArgumentNullException("refreshToken");

            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                var contentQueryString = new NameValueCollection();
                contentQueryString["client_id"] = "647667148.apps.googleusercontent.com";
                contentQueryString["client_secret"] = "SHvBqFmGtXq5bTPqY242oNvB";
                contentQueryString["refresh_token"] = refreshToken;
                contentQueryString["grant_type"] = "refresh_token";

                var resultBytes = webClient.UploadString(
                    "https://accounts.google.com/o/oauth2/token", ConvertToQueryString(contentQueryString));
                return new JavaScriptSerializer().Deserialize<AccessTokenJsonResult>(resultBytes);
            }
        }

        /// <summary>
        /// Retrieve an IAuthenticator instance using the provided state.
        /// </summary>
        /// <returns>Authenticator using the provided OAuth 2.0 credentials</returns>
        public static IAuthenticator GetAuthenticator(string refreshToken, string accessKey)
        {
            var credentials = new AuthorizationState(null)
            {
                RefreshToken = refreshToken,
                AccessToken = accessKey
            };

            var provider = new StoredStateClient(GoogleAuthenticationServer.Description, "647667148.apps.googleusercontent.com", "SHvBqFmGtXq5bTPqY242oNvB", credentials);
            var auth = new OAuth2Authenticator<StoredStateClient>(provider, StoredStateClient.GetState);
            auth.LoadAccessToken();
            return auth;
        }

        /// <summary>
        /// Update both metadata and content of a file and return the updated file.
        /// </summary>
        public static File UpdateFile(
            [NotNull] DriveService service,
            [NotNull] String fileId,
            [NotNull] String newTitle,
            [NotNull] String newDescription,
            [NotNull] String newMimeType, MemoryStream fileContent)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (fileId == null) throw new ArgumentNullException("fileId");
            if (newTitle == null) throw new ArgumentNullException("newTitle");
            if (newDescription == null) throw new ArgumentNullException("newDescription");
            if (newMimeType == null) throw new ArgumentNullException("newMimeType");
            // First retrieve the file from the API.
            var body = service.Files.Get(fileId).Fetch();

            body.Title = newTitle;
            body.Description = newDescription;
            body.MimeType = newMimeType;

            var request = service.Files.Update(body, fileId, fileContent, newMimeType);
            request.Upload();

            return request.ResponseBody;
        }

        /// <summary>
        /// Create a new file and return it.
        /// </summary>
        public static File CreateFile(
            [NotNull] DriveService service,
            [NotNull] String title,
            [NotNull] String description,
            [NotNull] String mimeType, [NotNull] MemoryStream fileContent, IList<ParentReference> parentReference = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (title == null) throw new ArgumentNullException("title");
            if (description == null) throw new ArgumentNullException("description");
            if (mimeType == null) throw new ArgumentNullException("mimeType");
            if (fileContent == null) throw new ArgumentNullException("fileContent");

            // File's metadata.
            var fileBody = new File
            {
                Title = title,
                Description = description,
                MimeType = mimeType,
                Parents = parentReference
            };

            var request = service.Files.Insert(fileBody, fileContent, mimeType);
            request.Upload();

            return request.ResponseBody;
        }

        public static File GetFile([NotNull] DriveService service, [NotNull] String fileId)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (fileId == null) throw new ArgumentNullException("fileId");

            return service.Files.Get(fileId).Fetch();
        }

        /// <summary>
        /// Create a new file and return it.
        /// </summary>
        public static File CreateFolder(
            [NotNull] DriveService service,
            [NotNull] String title,
            [NotNull] String description)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (title == null) throw new ArgumentNullException("title");
            if (description == null) throw new ArgumentNullException("description");

            var files = service.Files.List().Fetch();
            var pt = files.NextPageToken;


            // Folder's metadata.
            var folderBody = new File { Title = title, Description = description, MimeType = "application/vnd.google-apps.folder" };
            return service.Files.Insert(folderBody).Fetch();
        }

        /// <summary>
        /// Returns a query-string to be used as a content for POST requests
        /// </summary>
        /// <param name="nvc"></param>
        /// <returns></returns>
        private static string ConvertToQueryString(NameValueCollection nvc)
        {
            return string.Join(
                "&",
                Array.ConvertAll(nvc.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));
        }
    }
}