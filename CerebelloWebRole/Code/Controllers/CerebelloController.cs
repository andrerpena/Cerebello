using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello.Model;
using CerebelloWebRole.Code.Access;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Code.Data;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// This is the base Controller for all Controllers inside the App. 
    /// The site and documentation Controllers will NOT be CerebelloController
    /// </summary>
    public class CerebelloController : RootController
    {
        /// <summary>
        /// Object context used throughout all the controller
        /// </summary>
        public CerebelloEntitiesAccessFilterWrapper db = null;

        public User DbUser { get; private set; }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            this.InitDb();

            // Note that this is not CerebelloController responsability to ensure the user is logged in
            // or that the user exists, simply because there's no way to return anything here in the
            // Initialize method.
            // The responsability to ensure the user is authenticated and good to go is in the
            // AuthenticationFilter class.
            if (!this.Request.IsAuthenticated)
                return;

            this.InitDbUser(requestContext);

            if (this.DbUser != null)
            {
                // this ViewBag will carry user information to the View
                this.ViewBag.UserInfo = new UserInfo
                    {
                        Id = this.DbUser.Id,
                        DisplayName = this.DbUser.Person.FullName,
                        GravatarEmailHash = this.DbUser.Person.EmailGravatarHash,
                        // the following properties will only be set if the current user is a doctor
                        DoctorId = this.DbUser.DoctorId,
                        DoctorUrlIdentifier = this.DbUser.Doctor != null ? this.DbUser.Doctor.UrlIdentifier : null,
                        AdministratorId = this.DbUser.AdministratorId,
                        IsOwner = this.DbUser.IsOwner,
                    };
            }
        }

        internal CerebelloEntitiesAccessFilterWrapper InitDb()
        {
            // this is because of how tests work
            // if a test controller has been created, this.db has already been populated
            // otherwise let's create a regular one
            return this.db ?? (this.db = new CerebelloEntitiesAccessFilterWrapper(this.CreateNewCerebelloEntities()));
        }

        internal void InitDbUser(RequestContext requestContext)
        {
            if (this.DbUser == null)
            {
                if (!requestContext.HttpContext.Request.IsAuthenticated)
                    return;

                var authenticatedPrincipal = requestContext.HttpContext.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception(
                        "HttpContext.User should be a AuthenticatedPrincipal when the user is authenticated");

                var result = this.db.SetCurrentUserById(authenticatedPrincipal.Profile.Id);

                this.DbUser = result;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // if the base has already set a result, then we just exit this method
            if (filterContext.Result != null)
                return;

            Debug.Assert(this.DbUser != null, "this.DbUser must not be null");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.db.Dispose();
        }

        /// <summary>
        /// Returns a view indicating that the requested object does not exist in the database
        /// </summary>
        /// <returns></returns>
        public ActionResult ObjectNotFound()
        {
            return this.View("ObjectNotFound");
        }

        /// <summary>
        /// Returns a Json indicating that the requested object does not exist in the database
        /// </summary>
        /// <returns></returns>
        public JsonResult ObjectNotFoundJson()
        {
            return this.Json(new
                {
                    success = false,
                    text = "O registro solicitado não existe no banco de dados",
                },
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Creates an action result with a thumbnail image of a file in the storage.
        /// </summary>
        /// <param name="maxWidth">Maximum width of the thumbnail image.</param>
        /// <param name="maxHeight">Maximum height of the thumbnail image.</param>
        /// <param name="container">Container name of the source image file.</param>
        /// <param name="fileName">Name of the source image file.</param>
        /// <param name="thumbFileName">Name of the thumbnail image cache file.</param>
        /// <param name="useCache">Whether to use a cached thumbnail or not.</param>
        /// <returns>The ActionResult containing the thumbnail image.</returns>
        protected ActionResult GetOrCreateThumb(
            int maxWidth, int maxHeight, string container, string fileName, string thumbFileName, bool useCache = true)
        {
            byte[] array;
            string contentType;
            if (!TryGetOrCreateThumb(maxWidth, maxHeight, container, fileName, thumbFileName, useCache, out array, out contentType))
                return new StatusCodeResult(HttpStatusCode.NotFound);

            return this.File(array, contentType);
        }

        /// <summary>
        /// Creates a thumbnail image of a file in the storage.
        /// </summary>
        /// <param name="maxWidth">Maximum width of the thumbnail image.</param>
        /// <param name="maxHeight">Maximum height of the thumbnail image.</param>
        /// <param name="container">Container name of the source image file.</param>
        /// <param name="fileName">Name of the source image file.</param>
        /// <param name="thumbFileName">Name of the thumbnail image cache file.</param>
        /// <param name="useCache">Whether to use a cached thumbnail or not.</param>
        /// <param name="array">Array containing the thumbnail image bytes.</param>
        /// <param name="contentType">The content type of the data in the returned array.</param>
        /// <returns>True if thumbnail image exists; otherwise false.</returns>
        public static bool TryGetOrCreateThumb(
            int maxWidth,
            int maxHeight,
            string container,
            string fileName,
            string thumbFileName,
            bool useCache,
            out byte[] array,
            out string contentType)
        {
            var basePath = @"D:\Profile - MASB\Desktop\Cerebello.Debug\Storage\";
            var location = Path.Combine(basePath, container);
            var sourceFullFileName = Path.Combine(location, fileName);
            var thumbFullFileName = string.IsNullOrEmpty(thumbFileName) ? null : Path.Combine(location, thumbFileName);

            if (useCache && !string.IsNullOrEmpty(thumbFullFileName) && System.IO.File.Exists(thumbFullFileName))
            {
                using (var srcStream = System.IO.File.OpenRead(thumbFullFileName))
                using (var stream = new MemoryStream((int)srcStream.Length))
                {
                    srcStream.CopyTo(stream);
                    {
                        array = stream.ToArray();
                        contentType = MimeTypesHelper.GetContentType(Path.GetExtension(sourceFullFileName));
                        return true;
                    }
                }
            }

            var fileInfo = new FileInfo(sourceFullFileName);
            if (!fileInfo.Exists)
            {
                array = null;
                contentType = null;
                return false;
            }

            if (!StringHelper.IsImageFileName(fileName))
            {
                array = null;
                contentType = null;
                return false;
            }

            using (var srcStream = System.IO.File.OpenRead(sourceFullFileName))
            using (var srcImage = Image.FromStream(srcStream))
            using (var newImage = ImageHelper.ResizeImage(srcImage, maxWidth, maxHeight, keepAspect: true, canGrow: false))
            using (var newStream = new MemoryStream())
            {
                if (newImage == null)
                {
                    srcStream.Position = 0;
                    srcStream.CopyTo(newStream);
                    contentType = MimeTypesHelper.GetContentType(Path.GetExtension(sourceFullFileName));
                }
                else
                {
                    var imageFormat = (newImage.Width * newImage.Height > 10000)
                        ? ImageFormat.Jpeg
                        : ImageFormat.Png;

                    contentType = (newImage.Width * newImage.Height > 10000)
                        ? "image/jpeg"
                        : "image/png";

                    newImage.Save(newStream, imageFormat);
                }

                array = newStream.ToArray();

                if (useCache && newImage != null && !string.IsNullOrEmpty(thumbFullFileName))
                {
                    var dirThumb = Path.GetDirectoryName(thumbFullFileName);
                    if (dirThumb != null) Directory.CreateDirectory(dirThumb);

                    using (var thumbFileStream = System.IO.File.Create(thumbFullFileName))
                        thumbFileStream.Write(array, 0, array.Length);
                }
            }

            return true;
        }
    }
}
