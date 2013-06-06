using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello.Model;
using CerebelloWebRole.Code.Access;
using CerebelloWebRole.Code.Data;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Code.Services;

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
                this.ViewBag.UserInfo = CerebelloController.GetUserInfo(this.DbUser);
            }
        }

        internal static UserInfo GetUserInfo(User dbUser)
        {
            return new UserInfo
                {
                    Id = dbUser.Id,
                    DisplayName = dbUser.Person.FullName,
                    GravatarEmailHash = dbUser.Person.EmailGravatarHash,
                    // the following properties will only be set if the current user is a doctor
                    DoctorId = dbUser.DoctorId,
                    DoctorUrlIdentifier = dbUser.Doctor != null ? dbUser.Doctor.UrlIdentifier : null,
                    AdministratorId = dbUser.AdministratorId,
                    IsOwner = dbUser.IsOwner,
                };
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

                var db1 = this.InitDb();
                var result = db1.SetCurrentUserById(authenticatedPrincipal.Profile.Id);

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
        /// <param name="originalMetadata">Metadata of the original image file.</param>
        /// <param name="storage">The storage service used to store files.</param>
        /// <param name="dateTimeService">Date time service used to get current date and time.</param>
        /// <param name="maxWidth">Maximum width of the thumbnail image.</param>
        /// <param name="maxHeight">Maximum height of the thumbnail image.</param>
        /// <param name="useCache">Whether to use a cached thumbnail or not.</param>
        /// <returns>The ActionResult containing the thumbnail image.</returns>
        protected ActionResult GetOrCreateThumb(
            FileMetadata originalMetadata,
            IStorageService storage,
            IDateTimeService dateTimeService,
            int maxWidth,
            int maxHeight,
            bool useCache = true)
        {
            if (originalMetadata != null)
            {
                if (originalMetadata.OwnerUserId == this.DbUser.Id)
                {
                    var fileNamePrefix = Path.GetDirectoryName(originalMetadata.BlobName) + "\\";
                    var normalFileName = StringHelper.NormalizeFileName(originalMetadata.SourceFileName);

                    var thumbName = string.Format(
                        "{0}\\{1}file-{2}-thumb-{4}x{5}-{3}",
                        originalMetadata.ContainerName,
                        fileNamePrefix,
                        originalMetadata.Id,
                        normalFileName,
                        maxWidth,
                        maxHeight);

                    var fileName = string.Format("{0}\\{1}", originalMetadata.ContainerName, originalMetadata.BlobName);

                    return this.GetOrCreateThumb(
                        originalMetadata.Id, storage, dateTimeService, maxWidth, maxHeight, fileName, thumbName, useCache);
                }
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Creates an action result with a thumbnail image of a file in the storage.
        /// </summary>
        /// <param name="originalMetadataId">Metadata ID of the original image file.</param>
        /// <param name="storage">The storage service used to store files.</param>
        /// <param name="dateTimeService">Date time service used to get current date and time.</param>
        /// <param name="maxWidth">Maximum width of the thumbnail image.</param>
        /// <param name="maxHeight">Maximum height of the thumbnail image.</param>
        /// <param name="sourceFullStorageFileName">Name of the source image file.</param>
        /// <param name="thumbFullStorageFileName">Name of the thumbnail image cache file.</param>
        /// <param name="useCache">Whether to use a cached thumbnail or not.</param>
        /// <returns>The ActionResult containing the thumbnail image.</returns>
        protected ActionResult GetOrCreateThumb(
            int originalMetadataId,
            IStorageService storage,
            IDateTimeService dateTimeService,
            int maxWidth,
            int maxHeight,
            string sourceFullStorageFileName,
            string thumbFullStorageFileName,
            bool useCache = true)
        {
            var metadataProvider = new DbFileMetadataProvider(this.db, dateTimeService, this.DbUser.PracticeId);

            byte[] array;
            string contentType;

            if (!TryGetOrCreateThumb(
                originalMetadataId,
                maxWidth,
                maxHeight,
                sourceFullStorageFileName,
                thumbFullStorageFileName,
                useCache,
                storage,
                metadataProvider,
                out array,
                out contentType))
            {
                return new StatusCodeResult(HttpStatusCode.NotFound);
            }

            return this.File(array, contentType);
        }

        /// <summary>
        /// Creates a thumbnail image of a file in the storage.
        /// </summary>
        /// <param name="originalMetadataId">Metadata entry ID for the original image file.</param>
        /// <param name="maxWidth">Maximum width of the thumbnail image.</param>
        /// <param name="maxHeight">Maximum height of the thumbnail image.</param>
        /// <param name="sourceFullStorageFileName">Name of the source image file.</param>
        /// <param name="thumbFullStorageFileName">Name of the thumbnail image cache file.</param>
        /// <param name="useCache">Whether to use a cached thumbnail or not.</param>
        /// <param name="storage">Storage service used to get file data.</param>
        /// <param name="fileMetadataProvider">File metadata provider used to create thumbnail image metadata.</param>
        /// <param name="array">Array containing the thumbnail image bytes.</param>
        /// <param name="contentType">The content type of the data in the returned array.</param>
        /// <returns>True if thumbnail image exists; otherwise false.</returns>
        public static bool TryGetOrCreateThumb(
            int originalMetadataId,
            int maxWidth,
            int maxHeight,
            string sourceFullStorageFileName,
            string thumbFullStorageFileName,
            bool useCache,
            IStorageService storage,
            IFileMetadataProvider fileMetadataProvider,
            out byte[] array,
            out string contentType)
        {
            if (useCache && !string.IsNullOrEmpty(thumbFullStorageFileName) && storage.Exists(thumbFullStorageFileName))
            {
                using (var srcStream = storage.OpenRead(thumbFullStorageFileName))
                using (var stream = new MemoryStream((int)srcStream.Length))
                {
                    srcStream.CopyTo(stream);
                    {
                        array = stream.ToArray();
                        contentType = MimeTypesHelper.GetContentType(Path.GetExtension(thumbFullStorageFileName));
                        return true;
                    }
                }
            }

            if (!storage.Exists(sourceFullStorageFileName))
            {
                array = null;
                contentType = null;
                return false;
            }

            if (!StringHelper.IsImageFileName(sourceFullStorageFileName))
            {
                array = null;
                contentType = null;
                return false;
            }

            using (var srcStream = storage.OpenRead(sourceFullStorageFileName))
            using (var srcImage = Image.FromStream(srcStream))
            using (var newImage = ImageHelper.ResizeImage(srcImage, maxWidth, maxHeight, keepAspect: true, canGrow: false))
            using (var newStream = new MemoryStream())
            {
                if (newImage == null)
                {
                    srcStream.Position = 0;
                    srcStream.CopyTo(newStream);
                    contentType = MimeTypesHelper.GetContentType(Path.GetExtension(sourceFullStorageFileName));
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

                if (useCache && newImage != null && !string.IsNullOrEmpty(thumbFullStorageFileName))
                {
                    // saving thumbnail image file metadata
                    var containerName = thumbFullStorageFileName.Split("\\".ToCharArray(), 2).FirstOrDefault();
                    var sourceFileName = Path.GetFileName(thumbFullStorageFileName ?? "") ?? "";
                    var blobName = thumbFullStorageFileName.Split("\\".ToCharArray(), 2).Skip(1).FirstOrDefault();
                    var relationType = string.Format("thumb-{0}x{1}", maxWidth, maxHeight);
                    var metadata = fileMetadataProvider.CreateRelated(
                        originalMetadataId, relationType, containerName, sourceFileName, blobName, null);

                    fileMetadataProvider.SaveChanges();

                    using (var thumbFileStream = storage.CreateOrOverwrite(thumbFullStorageFileName))
                        thumbFileStream.Write(array, 0, array.Length);
                }
            }

            return true;
        }
    }
}
