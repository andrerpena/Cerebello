using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code;
using System.Linq;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class TempFileController : PracticeController
    {
        private readonly IStorageService storage;
        private readonly IDateTimeService datetimeService;

        public TempFileController(IStorageService storage, IDateTimeService datetimeService)
        {
            this.storage = storage;
            this.datetimeService = datetimeService;
        }

        [SelfPermission]
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS")]
        public ActionResult Index(int? id, string prefix, string location, string tag)
        {
            switch (this.Request.HttpMethod)
            {
                case "HEAD":
                case "GET":
                    //if (!string.IsNullOrWhiteSpace(fileName))
                    //    return this.DeliverFile(fileName);
                    //return this.ListCurrentFiles();
                    return this.Content("xxx");

                case "POST":
                case "PUT":
                    return this.UploadFile(prefix, location, tag);

                case "DELETE":
                    return this.DeleteFile(id ?? 0, location);

                case "OPTIONS":
                    //return this.ReturnOptions();
                    return this.Content("xxx");

                default:
                    return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }
        }

        /// <summary>
        /// Deletes an uploaded temporary file and all of its dependent files, such as thumbnail image files.
        /// </summary>
        /// <param name="fileId">File metadata id.</param>
        /// <param name="location">Location of the file in the storage. Final file name can be omitted.</param>
        /// <returns>Returns an action result indicating whether the file has been deleted.</returns>
        private ActionResult DeleteFile(int fileId, string location)
        {
            var metadata = this.db.FileMetadatas.SingleOrDefault(f => f.Id == fileId);

            if (metadata != null)
            {
                var containerName = location.Split("\\".ToCharArray(), 2).FirstOrDefault();

                if (metadata.OwnerUserId == this.DbUser.Id)
                {
                    if (containerName == metadata.ContainerName)
                        DeleteFileByMetadata(metadata, this.db, this.storage);

                    this.db.SaveChanges();

                    return new StatusCodeResult(HttpStatusCode.OK);
                }
            }

            return new StatusCodeResult(HttpStatusCode.NotFound, "Arquivo não encontrado.");
        }

        public static void DeleteFileByMetadata(FileMetadata metadata, CerebelloEntitiesAccessFilterWrapper db, IStorageService storage)
        {
            var dbLocation = string.Format("{0}\\{1}", metadata.ContainerName, metadata.BlobName);

            // deleting dependent files
            var relatedFiles = db.FileMetadatas.Where(f => f.RelatedFileMetadataId == metadata.Id).ToList();
            foreach (var relatedFile in relatedFiles)
            {
                DeleteFileByMetadata(relatedFile, db, storage);
            }

            // deleting file metadata and storage entries
            storage.DeleteBlob(dbLocation);
            db.FileMetadatas.DeleteObject(metadata);
        }

        private ActionResult UploadFile(string prefix, string location, string tag)
        {
            var headerXFileName = this.Request.Headers["X-File-Name"];

            if (string.IsNullOrEmpty(headerXFileName))
                return this.UploadWholeFile(prefix, location, tag);

            return this.UploadPartialFile(headerXFileName, prefix, location, tag);
        }

        /// <summary>
        /// Upload whole file.
        /// </summary>
        /// <param name="prefix"> The prefix of the fields to be placed in the HTML. </param>
        /// <param name="location"> The location where the temporary file should be stored. </param>
        /// <returns> The <see cref="ActionResult"/> containing information about execution of the upload. </returns>
        private ActionResult UploadWholeFile(string prefix, string location, string tag)
        {
            var statuses = new List<FilesStatus>();

            for (int i = 0; i < this.Request.Files.Count; i++)
            {
                var file = this.Request.Files[i];

                Debug.Assert(file != null, "file != null");

                var containerName = location.Split("\\".ToCharArray(), 2).FirstOrDefault();
                var sourceFileName = Path.GetFileName(file.FileName ?? "") ?? "";
                var normalFileName = StringHelper.NormalizeFileName(sourceFileName);
                var fileNamePrefix = location.Split("\\".ToCharArray(), 2).Skip(1).FirstOrDefault();
                var fileExpirationDate = this.datetimeService.UtcNow + TimeSpan.FromDays(file.ContentLength < 10 * 1024000 ? 2 : 10);

                Debug.Assert(sourceFileName != null, "sourceFileName != null");

                var metadataProvider = new DbFileMetadataProvider(this.db, this.datetimeService, this.DbUser.PracticeId);

                // creating the metadata entry for the main file
                FileMetadata metadata = metadataProvider.CreateTemporary(
                    containerName,
                    sourceFileName,
                    string.Format("{0}file-{1}-{2}", fileNamePrefix, "{id}", normalFileName),
                    fileExpirationDate,
                    this.DbUser.Id,
                    tag,
                    formatWithId: true);

                metadata.OwnerUserId = this.DbUser.Id;

                metadataProvider.SaveChanges();

                // saving the file to the storage
                var fullStoragePath = string.Format("{0}\\{1}", containerName, metadata.BlobName);
                this.storage.SaveFile(file.InputStream, fullStoragePath);

                // returning information to the client
                var fileStatus = new FilesStatus(metadata.Id, sourceFileName, file.ContentLength, prefix);

                bool imageThumbOk = false;
                try
                {
                    var thumbName = string.Format("{0}\\{1}file-{2}-thumb-{4}x{5}-{3}", containerName, fileNamePrefix, metadata.Id, normalFileName, 80, 80);
                    var thumbResult = ImageHelper.TryGetOrCreateThumb(metadata.Id, 80, 80, fullStoragePath, thumbName, true, this.storage, metadataProvider);
                    if (thumbResult.Status == CreateThumbStatus.Ok)
                    {
                        fileStatus.ThumbnailUrl = @"data:" + thumbResult.ContentType + ";base64," + Convert.ToBase64String(thumbResult.Data);
                        fileStatus.IsInGallery = true;
                        imageThumbOk = true;
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                }

                if (!imageThumbOk)
                {
                    if (StringHelper.IsDocumentFileName(sourceFileName))
                    {
                        fileStatus.IconClass = "document-file-icon";
                    }
                    else
                    {
                        fileStatus.IconClass = "generic-file-icon";
                    }
                }
                else
                {
                    fileStatus.UrlLarge = this.Url.Action("Image", new { w = 1024, h = 768, location, metadata.Id });
                }

                fileStatus.UrlFull = this.Url.Action("File", new { location, metadata.Id });

                fileStatus.DeleteUrl = this.Url.Action("Index", new { location, metadata.Id });

                statuses.Add(fileStatus);
            }

            return this.JsonIframeSafe(new { files = statuses });
        }

        /// <summary>
        /// Uploads a partial file to the server.
        /// </summary>
        /// <param name="fileName"> The file name. </param>
        /// <param name="prefix"> The prefix of the fields to be placed in the HTML. </param>
        /// <param name="location"> The storage location for the temporary file. </param>
        /// <returns> The <see cref="ActionResult"/> containing information about execution of the upload. </returns>
        /// <exception cref="HttpRequestValidationException"> When more than one file is uploaded, or when the file is null. </exception>
        private ActionResult UploadPartialFile(string fileName, string prefix, string location, string tag)
        {
            // todo: partial file upload is not yet supported

            if (this.Request.Files.Count != 1)
                throw new HttpRequestValidationException(
                    "Attempt to upload chunked file containing more than one fragment per request");

            var httpPostedFileBase = this.Request.Files[0];
            if (httpPostedFileBase == null)
                throw new HttpRequestValidationException("Posted file is null.");

            var inputStream = httpPostedFileBase.InputStream;

            fileName = Path.GetFileName(fileName);
            Debug.Assert(fileName != null, "fileName != null");
            var fullPath = Path.Combine(location, fileName);

            this.storage.AppendToFile(inputStream, fullPath);

            var fileLength = (long)this.storage.GetFileLength(fullPath);

            // todo: must create a valid file metadata, or retrieve an already existing one from the database
            int id = 0;
            var fileStatus = new FilesStatus(id, fileName, fileLength, prefix);

            // when doing partial upload of a file, no thumb-image will be generated
            // also the file wont display in the gallery (because no url will be provided)
            if (StringHelper.IsDocumentFileName(fileName))
            {
                fileStatus.IconClass = "document-file-icon";
            }
            else
            {
                fileStatus.IconClass = "generic-file-icon";
            }

            fileStatus.UrlLarge = null;

            // cannot download when it is a partial uploaded file
            fileStatus.UrlFull = null;

            return this.JsonIframeSafe(new { files = new[] { fileStatus } });
        }

        /// <summary>
        /// Returns a JsonResult that can be rendered inside an IFrame without the browser asking to save the Json.
        /// </summary>
        /// <param name="obj">Object to be Json serialized.</param>
        /// <returns>Returns a JsonResult, containing the passed object serialized as a Json.</returns>
        private JsonResult JsonIframeSafe(object obj)
        {
            this.Response.AddHeader("Vary", "Accept");

            string contentType;
            try
            {
                contentType = this.Request["HTTP_ACCEPT"].Contains("application/json")
                    ? "application/json"
                    : "text/plain";
            }
            catch
            {
                contentType = "text/plain";
            }

            return this.Json(obj, contentType, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets a thumbnail image for a temporary image file.
        /// </summary>
        /// <param name="w">Width of the thumbnail image.</param>
        /// <param name="h">Height of the thumbnail image.</param>
        /// <param name="location">Location inside the temp container in the storage.</param>
        /// <param name="id">Metadata ID of the file to generate thumbnail image to.</param>
        /// <returns>Returns an ActionResult containing the thumbnail image data.</returns>
        [SelfPermission]
        public ActionResult Image(int w, int h, string location, int id)
        {
            var metadata = this.db.FileMetadatas.SingleOrDefault(f => f.Id == id);
            var containerName = location.Split("\\".ToCharArray(), 2).FirstOrDefault();

            if (metadata != null)
                if (containerName == metadata.ContainerName && metadata.OwnerUserId == this.DbUser.Id)
                    return this.GetOrCreateThumb(metadata, this.storage, this.datetimeService, w, h);

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        [SelfPermission]
        public ActionResult File(string location, int id)
        {
            var metadata = this.db.FileMetadatas.SingleOrDefault(f => f.Id == id);

            if (metadata != null)
            {
                var containerName = location.Split("\\".ToCharArray(), 2).FirstOrDefault();

                if (containerName == metadata.ContainerName)
                {
                    var fullStoragePath = string.Format("{0}\\{1}", containerName, metadata.BlobName);
                    var fileName = metadata.SourceFileName;
                    var stream = this.storage.OpenRead(fullStoragePath);

                    if (stream == null)
                        return new StatusCodeResult(HttpStatusCode.NotFound);

                    using (stream)
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        return this.File(memoryStream.ToArray(), MimeTypesHelper.GetContentType(fileName), fileName);
                    }
                }
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        [SelfPermission]
        public ActionResult DeleteTempFiles(string location, string tag)
        {
            // locating the temporary file by tag
            var containerName = location.Split("\\".ToCharArray(), 2).FirstOrDefault();
            var mds = this.db.FileMetadatas
                .Where(f => f.ContainerName == containerName && f.Tag == tag)
                .Where(f => f.ExpirationDate != null)
                .ToList();

            foreach (var fileMetadata in mds)
                if (fileMetadata.OwnerUserId == this.DbUser.Id)
                    DeleteFileByMetadata(fileMetadata, this.db, this.storage);

            this.db.SaveChanges();

            return this.Content("OK");
        }

        public override bool IsSelfUser(User user)
        {
            // using request parameters that may contain the user-id
            var location = this.Request.Params["location"];

            if (location.StartsWith(string.Format(@"patient-files-{0}\", user.Id)))
                return true;

            return base.IsSelfUser(user);
        }

        public class FilesStatus
        {
            /// <summary>
            /// Gets or sets the name of the file.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the mime type of the file.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the size of the file.
            /// </summary>
            public long? Size { get; set; }

            /// <summary>
            /// Gets or sets the Url of the large preview image that is used in the gallery preview.
            /// </summary>
            public string UrlLarge { get; set; }

            /// <summary>
            /// Gets or sets the Url of the full image file.
            /// </summary>
            public string UrlFull { get; set; }

            /// <summary>
            /// Gets or sets the Url of the thumbnail image.
            /// </summary>
            public string ThumbnailUrl { get; set; }

            /// <summary>
            /// Gets or sets the Url used to delete the file when user wants to.
            /// This is optional, and if left empty the server must not be called upon deletion.
            /// </summary>
            public string DeleteUrl { get; set; }

            /// <summary>
            /// Gets or sets the non-image file icon class.
            /// </summary>
            public string IconClass { get; set; }

            /// <summary>
            /// Gets or sets the error message to display when errors happen.
            /// </summary>
            public string Error { get; set; }

            /// <summary>
            /// Gets or sets whether this file should be displayed in the gallert or not.
            /// Only image files should be displayed in the gallery.
            /// </summary>
            public bool IsInGallery { get; set; }

            public string Index { get; set; }
            public string IndexFieldName { get; set; }

            public string FieldNamePrefix { get; set; }

            public int? MetadataId { get; set; }
            public string FileName { get; set; }

            public FilesStatus(int id, string fileName, long? fileLength, string prefix)
            {
                this.Name = fileName;
                this.Type = "image/png";
                this.Size = fileLength;

                var itemPrefixBase = prefix + (string.IsNullOrEmpty(prefix) ? "" : ".") + "Files";
                var indexStr = id.ToString(CultureInfo.InvariantCulture);
                this.FieldNamePrefix = itemPrefixBase + "[" + indexStr + "]";

                this.IndexFieldName = itemPrefixBase + ".Index";
                this.Index = indexStr;

                this.MetadataId = id;

                this.FileName = fileName;
            }
        }
    }
}
