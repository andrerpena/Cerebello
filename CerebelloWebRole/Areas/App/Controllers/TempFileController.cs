using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class TempFileController : PracticeController
    {
        [SelfPermission]
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS")]
        public ActionResult Index(string fileId, string prefix, string tempLocation)
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
                    return this.UploadFile(prefix, tempLocation);

                case "DELETE":
                    //return this.DeleteFile();
                    return this.Content("xxx");

                case "OPTIONS":
                    //return this.ReturnOptions();
                    return this.Content("xxx");

                default:
                    return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }
        }

        private ActionResult UploadFile(string prefix, string tempLocation)
        {
            var headerXFileName = this.Request.Headers["X-File-Name"];

            if (string.IsNullOrEmpty(headerXFileName))
                return this.UploadWholeFile(prefix, tempLocation);

            return this.UploadPartialFile(headerXFileName, prefix, tempLocation);
        }

        /// <summary>
        /// Upload whole file.
        /// </summary>
        /// <param name="prefix"> The prefix of the fields to be placed in the HTML. </param>
        /// <param name="tempLocation"> The temp storage location. </param>
        /// <returns> The <see cref="ActionResult"/> containing information about execution of the upload. </returns>
        private ActionResult UploadWholeFile(string prefix, string tempLocation)
        {
            var statuses = new List<FilesStatus>();

            for (int i = 0; i < this.Request.Files.Count; i++)
            {
                var file = this.Request.Files[i];

                Debug.Assert(file != null, "file != null");
                var location = Path.Combine(Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME, tempLocation);

                FileHelper.CreateDirectory(location);

                var fileName = Path.GetFileName(file.FileName);
                Debug.Assert(fileName != null, "fileName != null");
                var fullPath = Path.Combine(location, fileName);

                FileHelper.SavePostedFile(file, fullPath);

                string fullName = Path.GetFileName(file.FileName);

                var fileStatus = new FilesStatus(fullName, file.ContentLength, prefix, location);

                bool imageThumbOk = false;
                try
                {
                    var thumbName = string.Format(@"thumbs-{0}x{1}\{2}", 80, 80, fileName);
                    byte[] array;
                    string contentType;
                    bool thumbExists = TryGetOrCreateThumb(80, 80, location, fileName, thumbName, true, out array, out contentType);
                    if (thumbExists)
                    {
                        fileStatus.ThumbnailUrl = @"data:" + contentType + ";base64," + Convert.ToBase64String(array);
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
                    if (StringHelper.IsDocumentFileName(fileName))
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
                    fileStatus.UrlLarge = this.Url.Action("Thumb", new { w = 1024, h = 768, tempLocation, fileName });
                }

                fileStatus.UrlFull = this.Url.Action("File", new { tempLocation, fileName });

                statuses.Add(fileStatus);
            }

            return this.JsonIframeSafe(new { files = statuses });
        }

        /// <summary>
        /// Uploads a partial file to the server.
        /// </summary>
        /// <param name="fileName"> The file name. </param>
        /// <param name="prefix"> The prefix of the fields to be placed in the HTML. </param>
        /// <param name="tempLocation"> The temp storage location. </param>
        /// <returns> The <see cref="ActionResult"/> containing information about execution of the upload. </returns>
        /// <exception cref="HttpRequestValidationException"> When more than one file is uploaded, or when the file is null. </exception>
        private ActionResult UploadPartialFile(string fileName, string prefix, string tempLocation)
        {
            if (this.Request.Files.Count != 1)
                throw new HttpRequestValidationException(
                    "Attempt to upload chunked file containing more than one fragment per request");

            var httpPostedFileBase = this.Request.Files[0];
            if (httpPostedFileBase == null)
                throw new HttpRequestValidationException("Posted file is null.");

            var inputStream = httpPostedFileBase.InputStream;

            var location = Path.Combine(Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME, tempLocation);

            Directory.CreateDirectory(location);

            fileName = Path.GetFileName(fileName);
            Debug.Assert(fileName != null, "fileName != null");
            var fullPath = Path.Combine(location, fileName);

            long fileLength;
            var fileStream = FileHelper.OpenAppend(fullPath);

            if (fileStream == null)
                return new StatusCodeResult(HttpStatusCode.NotFound);

            using (fileStream)
            {
                inputStream.CopyTo(fileStream);
                fileLength = inputStream.Position;
            }

            var fileStatus = new FilesStatus(fileName, fileLength, prefix, location);

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
        /// <param name="tempLocation">Location inside the temp container in the storage.</param>
        /// <param name="fileName">Name of the temporary image file to generate the thumbnail image from.</param>
        /// <returns>Returns an ActionResult containing the thumbnail image data.</returns>
        [SelfPermission]
        public ActionResult Thumb(int w, int h, string tempLocation, string fileName)
        {
            var location = Path.Combine(Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME, tempLocation);
            var thumbName = string.Format(@"thumbs-{0}x{1}\{2}", w, h, fileName);
            return this.GetOrCreateThumb(w, h, location, fileName, thumbName);
        }

        [SelfPermission]
        public ActionResult File(string tempLocation, string fileName)
        {
            var location = Path.Combine(Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME, tempLocation);
            var fullFileName = Path.Combine(location, fileName);

            var stream = FileHelper.OpenRead(fullFileName);

            if (stream == null)
                return new StatusCodeResult(HttpStatusCode.NotFound);

            using (stream)
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return this.File(memoryStream.ToArray(), MimeTypesHelper.GetContentType(fileName), fileName);
            }
        }

        public override bool IsSelfUser(Cerebello.Model.User user)
        {
            // using request parameters that may contain the user-id
            var tempLocation = this.Request.Params["tempLocation"];

            if (tempLocation.StartsWith(string.Format(@"patient-files-{0}-", user.Id)))
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

            public string progress { get; set; }

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

            public string IdFieldName { get; set; }
            public string FileTitleFieldName { get; set; }
            public string FileNameFieldName { get; set; }
            public string FileContainerFieldName { get; set; }

            public int? Id { get; set; }
            public string FileTitle { get; set; }
            public string FileName { get; set; }
            public string FileContainer { get; set; }

            public string Index { get; set; }
            public string IndexFieldName { get; set; }

            public FilesStatus(string fileName, long? fileLength, string prefix, string location, string fileTitle = null, int? id = null)
            {
                this.Name = fileName;
                this.Type = "image/png";
                this.Size = fileLength;
                this.progress = "1.0";

                var itemPrefixBase = prefix + (string.IsNullOrEmpty(prefix) ? "" : ".") + "Files";
                var indexStr = id == null ? Guid.NewGuid().ToString() : id.Value.ToString();
                var itemPrefix = itemPrefixBase + "[" + indexStr + "]";
                var templateInfo = new TemplateInfo { HtmlFieldPrefix = itemPrefix };

                this.IndexFieldName = itemPrefixBase + ".Index";
                this.Index = indexStr;

                this.IdFieldName = templateInfo.GetFullHtmlFieldName("Id");
                this.Id = id;

                this.FileName = fileName;
                this.FileNameFieldName = templateInfo.GetFullHtmlFieldName("FileName");

                this.FileContainer = location;
                this.FileContainerFieldName = templateInfo.GetFullHtmlFieldName("FileContainer");

                this.FileTitle = fileTitle;
                this.FileTitleFieldName = templateInfo.GetFullHtmlFieldName("FileTitle");
            }
        }
    }
}
