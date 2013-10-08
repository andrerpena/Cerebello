using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Helpers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using Ionic.Zip;
using JetBrains.Annotations;
using File = Cerebello.Model.FileMetadata;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Manages patient files
    /// </summary>
    public class PatientFilesController : DoctorController
    {
        private readonly IBlobStorageManager storage;
        private readonly IDateTimeService datetimeService;

        public PatientFilesController(IBlobStorageManager storage, IDateTimeService datetimeService)
        {
            this.storage = storage;
            this.datetimeService = datetimeService;
        }

        [SelfPermission]
        public ActionResult DownloadBackup(int patientId)
        {
            var patient = this.db.Patients.First(p => p.Id == patientId);
            var backup = BackupHelper.GeneratePatientBackup(this.db, patient);
            return this.File(
                backup,
                "application/zip",
                PersonHelper.GetFullName(patient.Person) + " - " + this.GetPracticeLocalNow().ToShortDateString() + ".zip");
        }

        /// <summary>
        /// Downloads a zip file with all patient files
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        [HttpGet]
        [SelfPermission]
        public ActionResult DownloadZipFile(int patientId)
        {
            var patient = this.db.Patients.First(p => p.Id == patientId);
            var zipMemoryStream = new MemoryStream();

            using (var zip = new ZipFile())
            {
                var patientFiles = this.db.PatientFiles.Where(pf => pf.PatientId == patientId).ToList();
                var storageManager = new WindowsAzureBlobStorageManager();

                foreach (var patientFile in patientFiles)
                {
                    var fileStream = storageManager.DownloadFileFromStorage(
                        patientFile.FileMetadata.ContainerName, patientFile.FileMetadata.BlobName);

                    zip.AddEntry(patientFile.FileMetadata.SourceFileName, fileStream);
                }
                zip.Save(zipMemoryStream);
            }

            zipMemoryStream.Seek(0, SeekOrigin.Begin);
            return this.File(
                zipMemoryStream,
                "application/zip",
               PersonHelper.GetFullName(patient.Person) + " - Arquivos - " + this.GetPracticeLocalNow().ToShortDateString() + ".zip");
        }

        /// <summary>
        /// Downloads a zip file with all files from all patients.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SelfPermission]
        public ActionResult DownloadAllPatientsZipFile()
        {
            var mainZipMemoryStream = new MemoryStream();

            // there will an outer zip file that will contain an inner 
            // zip file for each patient that has files
            using (var outerZip = new ZipFile())
            {
                foreach (var patient in this.Doctor.Patients)
                {
                    // if the patient has no files, he's not going to be included
                    if (!patient.PatientFiles.Any())
                        continue;

                    var innerZipMemoryStream = new MemoryStream();
                    using (var innerZip = new ZipFile())
                    {
                        var storageManager = new WindowsAzureBlobStorageManager();

                        foreach (var patientFile in patient.PatientFiles)
                        {
                            try
                            {
                                var fileStream = storageManager.DownloadFileFromStorage(
                                    patientFile.FileMetadata.ContainerName, patientFile.FileMetadata.BlobName);

                                innerZip.AddEntry(patientFile.FileMetadata.SourceFileName, fileStream);
                            }
                            catch (Exception ex)
                            {
                                // in this case the file exists in the database but does not exist in the storage.
                                Trace.TraceError(ex.Message);
                            }
                        }

                        innerZip.Save(innerZipMemoryStream);
                    }

                    innerZipMemoryStream.Seek(0, SeekOrigin.Begin);
                    outerZip.AddEntry(PersonHelper.GetFullName(patient.Person) + ".zip", innerZipMemoryStream);
                }

                outerZip.Save(mainZipMemoryStream);
            }

            mainZipMemoryStream.Seek(0, SeekOrigin.Begin);
            return this.File(
                mainZipMemoryStream,
                "application/zip",
                PersonHelper.GetFullName(this.Doctor.Users.ElementAt(0).Person) + " - Patient files - " +
                    this.GetPracticeLocalNow().ToShortDateString() + ".zip");
        }

        private static PatientFilesGroupViewModel GetViewModel(IBlobStorageManager storage, PatientFileGroup dbFileGroup, int dbUserId, Func<DateTime, DateTime> toLocal)
        {
            if (dbFileGroup == null)
                return new PatientFilesGroupViewModel();

            var result = new PatientFilesGroupViewModel
                {
                    Id = dbFileGroup.Id,
                    PatientId = dbFileGroup.PatientId,
                    CreatedOn = dbFileGroup.CreatedOn,
                    Title = dbFileGroup.GroupTitle,
                    Notes = dbFileGroup.GroupNotes,
                    FileGroupDate = toLocal(dbFileGroup.FileGroupDate),
                    ReceiveDate = toLocal(dbFileGroup.ReceiveDate),
                };

            result.Files.AddRange(dbFileGroup.PatientFiles.Select(dbFile => new PatientFileViewModel
                {
                    Id = dbFile.Id,
                    FileTitle = dbFile.Title,
                    ContainerName = dbFile.FileMetadata.ContainerName,
                    SourceFileName = dbFile.FileMetadata.SourceFileName,
                    BlobName = dbFile.FileMetadata.BlobName,
                    ExpirationDate = dbFile.FileMetadata.ExpirationDate,
                    MetadataId = dbFile.FileMetadataId,
                }));

            FillFileLengths(storage, result, dbUserId);

            return result;
        }

        private static void FillFileLengths(IBlobStorageManager storage, PatientFilesGroupViewModel viewModel, int? dbUserId)
        {
            // reading file sizes from the storage
            // todo: use db to get file size (faster)
            foreach (var eachFile in viewModel.Files)
            {
                var fullStoragePath = string.Format("{0}\\{1}", eachFile.ContainerName, eachFile.BlobName);
                var mustStartWith = string.Format("patient-files-{0}\\", dbUserId);
                if (!fullStoragePath.StartsWith(mustStartWith))
                    continue;

                eachFile.FileLength = storage.GetFileLength(eachFile.ContainerName, eachFile.BlobName);
            }
        }

        public class FilesStatus : TempFileController.FilesStatus
        {
            public FilesStatus(int? id, int metadataId, string fileName, long? fileLength, string prefix, string fileTitle)
                : base(metadataId, fileName, fileLength, prefix)
            {
                this.PatientFileId = id;
                this.FileTitle = fileTitle;
            }

            public int? PatientFileId { get; set; }
            public string FileTitle { get; set; }
        }

        public FilesStatus GetFilesStatus(PatientFileViewModel fileModel, string prefix)
        {
            var fileName = fileModel.SourceFileName;

            var containerName = fileModel.ContainerName;
            var sourceFileName = Path.GetFileName(fileModel.SourceFileName ?? "") ?? "";
            var normalFileName = StringHelper.RemoveDiacritics(sourceFileName.ToLowerInvariant());
            var fileNamePrefix = Path.GetDirectoryName(fileModel.BlobName) + "\\";

            var fullStoragePath = string.Format("{0}\\{1}file-{2}-{3}", containerName, fileNamePrefix, fileModel.Id, normalFileName);

            var fileStatus = new FilesStatus(fileModel.Id, fileModel.MetadataId, fileName, fileModel.FileLength, prefix, fileModel.FileTitle);

            var isPatientFiles = Regex.IsMatch(fileModel.ContainerName, @"^patient-files-\d+$");

            // Validating each file location... otherwise this could be a security hole.
            if (!isPatientFiles)
                throw new Exception("Invalid file location for patient files.");

            var fileMetadataProvider = new DbFileMetadataProvider(this.db, this.datetimeService, this.DbUser.PracticeId);

            bool imageThumbOk = false;
            try
            {
                var thumbName = string.Format("{0}\\{1}file-{2}-thumb-{4}x{5}-{3}", containerName, fileNamePrefix, fileModel.MetadataId, normalFileName, 120, 120);
                var thumbResult = ImageHelper.TryGetOrCreateThumb(fileModel.MetadataId, 120, 120, fullStoragePath, thumbName, true, storage, fileMetadataProvider);
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
                if (StringHelper.IsDocumentFileName(fileName))
                {
                    fileStatus.IconClass = "document-file-icon";
                }
                else
                {
                    fileStatus.IconClass = "generic-file-icon";
                }
            }

            bool isTemp = fileModel.Id == null;
            if (isTemp)
            {
                var location = string.Format("{0}\\{1}", fileModel.ContainerName, fileModel.BlobName);
                if (imageThumbOk)
                    fileStatus.UrlLarge = this.Url.Action("Image", "TempFile", new { w = 1024, h = 768, id = fileModel.MetadataId, location });

                fileStatus.UrlFull = this.Url.Action("File", "TempFile", new { id = fileModel.MetadataId, location });
            }
            else
            {
                if (imageThumbOk)
                    fileStatus.UrlLarge = this.Url.Action("Image", "PatientFiles", new { w = 1024, h = 768, id = fileModel.Id });

                fileStatus.UrlFull = this.Url.Action("File", "PatientFiles", new { id = fileModel.Id });
            }

            return fileStatus;
        }

        public ActionResult Details(int id)
        {
            var patientFileGroup = this.db.PatientFileGroups.First(pf => pf.Id == id);
            return this.View(GetViewModel(this.storage, patientFileGroup, this.DbUser.Id, this.GetToLocalDateTimeConverter()));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey, int? y, int? m, int? d)
        {
            return this.Edit(null, patientId, y, m, d);
        }

        [HttpPost]
        public ActionResult Create(Dictionary<string, PatientFilesGroupViewModel> patientFilesGroups)
        {
            return this.Edit(patientFilesGroups);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId, int? y, int? m, int? d)
        {
            PatientFilesGroupViewModel viewModel = null;

            if (id != null)
            {
                viewModel = GetViewModel(
                    this.storage,
                    (from pf in this.db.PatientFileGroups where pf.Id == id select pf).First(),
                    this.DbUser.Id,
                    this.GetToLocalDateTimeConverter());
            }
            else
            {
                Debug.Assert(patientId != null, "patientId != null");
                viewModel = new PatientFilesGroupViewModel
                    {
                        Id = null,
                        PatientId = patientId.Value,
                        FileGroupDate = null,
                        ReceiveDate = DateTimeHelper.CreateDate(y, m, d) ?? this.GetPracticeLocalNow(),
                    };
            }

            viewModel.NewGuid = Guid.NewGuid();

            this.ViewBag.FilesStatusGetter = (FilesStatusGetter)this.GetFilesStatus;

            return this.View("Edit", viewModel);
        }

        public delegate FilesStatus FilesStatusGetter(PatientFileViewModel fileModel, string prefix);

        [HttpPost]
        public ActionResult Edit(Dictionary<string, PatientFilesGroupViewModel> patientFilesGroups)
        {
            var kv = patientFilesGroups.Single();
            var formModel = kv.Value;

            PatientFileGroup dbFileGroup;

            if (formModel.Id == null)
            {
                Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
                dbFileGroup = new PatientFileGroup
                    {
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                        CreatedOn = this.datetimeService.UtcNow,
                    };

                this.db.PatientFileGroups.AddObject(dbFileGroup);
            }
            else
            {
                dbFileGroup = this.db.PatientFileGroups
                    .Include("PatientFiles")
                    .Include("PatientFiles.FileMetadata")
                    .FirstOrDefault(pe => pe.Id == formModel.Id);
            }

            Debug.Assert(dbFileGroup != null, "dbFileGroup != null");
            var allExistingFilesInGroup = dbFileGroup.PatientFiles.ToDictionary(pf => pf.Id);

            var idsToKeep = new HashSet<int>(formModel.Files.Where(f => f.Id != null).Select(f => f.Id.Value));

            var storageActions = new List<Action>(formModel.Files.Count);

            var metadataProvider = new DbFileMetadataProvider(this.db, this.datetimeService, this.DbUser.PracticeId);
            var metadataDic = metadataProvider.GetByIds(formModel.Files.Select(f => f.MetadataId).ToArray()).ToDictionary(f => f.Id);

            foreach (var eachFile in formModel.Files)
            {
                // Validating each file location... otherwise this could be a security hole.
                FileMetadata metadata;
                metadataDic.TryGetValue(eachFile.MetadataId, out metadata);

                if (metadata == null)
                    return new StatusCodeResult(HttpStatusCode.NotFound, "Arquivo não encontrado. Outra pessoa deve ter removido esse arquivo neste instante.");

                var validContainer = string.Format("patient-files-{0}", this.DbUser.Id);

                if (metadata.ContainerName != validContainer)
                    throw new Exception("Invalid file location.");

                PatientFile patientFile;

                if (eachFile.Id == null)
                {
                    // creating and adding the new patient file
                    Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");

                    patientFile = new PatientFile
                    {
                        FileMetadataId = metadata.Id,
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };

                    dbFileGroup.PatientFiles.Add(patientFile);

                    // changing file metadata:
                    // - it is not temporary anymore
                    // - tag is free for another operation
                    metadata.ExpirationDate = null;
                    metadata.Tag = null;
                }
                else if (!allExistingFilesInGroup.TryGetValue(eachFile.Id.Value, out patientFile))
                {
                    return new StatusCodeResult(HttpStatusCode.NotFound, "Arquivo não encontrado. Outra pessoa deve ter removido esse arquivo neste instante.");
                }

                Debug.Assert(patientFile != null, "patientFile != null");

                patientFile.Title = eachFile.FileTitle;
            }

            // deleting files that were removed
            foreach (var patientFileKv in allExistingFilesInGroup)
            {
                if (!idsToKeep.Contains(patientFileKv.Key))
                {
                    // create delegate to kill the file metadata and the storage blob
                    // this is going to be called latter
                    var metadata = patientFileKv.Value.FileMetadata;
                    Action removeFile = () => TempFileController.DeleteFileByMetadata(metadata, this.db, this.storage);
                    storageActions.Add(removeFile);

                    // delete patient file (note that changes are not being saved yet)
                    this.db.PatientFiles.DeleteObject(patientFileKv.Value);
                }
            }

            if (formModel.Files.Count == 0)
            {
                this.ModelState.AddModelError(string.Format("PatientFilesGroups[{0}].Files", kv.Key), "Deve haver pelo menos um arquivo na lista.");
            }

            if (this.ModelState.IsValid)
            {
                dbFileGroup.GroupTitle = formModel.Title;
                dbFileGroup.GroupNotes = formModel.Notes;
                Debug.Assert(formModel.FileGroupDate != null, "formModel.FileGroupDate != null");
                dbFileGroup.FileGroupDate = this.ConvertToUtcDateTime(formModel.FileGroupDate.Value);
                Debug.Assert(formModel.ReceiveDate != null, "formModel.ReceiveDate != null");
                dbFileGroup.ReceiveDate = this.ConvertToUtcDateTime(formModel.ReceiveDate.Value);

                dbFileGroup.Patient.IsBackedUp = false;
                this.db.SaveChanges();

                // moving files that are stored in a temporary location
                foreach (var moveAction in storageActions)
                    moveAction();

                return this.View("Details", GetViewModel(this.storage, dbFileGroup, this.DbUser.Id, this.GetToLocalDateTimeConverter()));
            }

            FillMissingInfos(formModel, this.db.FileMetadatas);
            FillFileLengths(this.storage, formModel, this.DbUser.Id);

            this.ViewBag.FilesStatusGetter = (FilesStatusGetter)this.GetFilesStatus;

            return this.View("Edit", formModel);
        }

        private void FillMissingInfos(PatientFilesGroupViewModel formModel, IObjectSet<FileMetadata> dbFileMetadataSet)
        {
            var ids = formModel.Files.Select(f => f.MetadataId).ToArray();
            var filesInGroup = dbFileMetadataSet
                .Where(f => ids.Contains(f.Id))
                .ToDictionary(f => f.Id);

            foreach (var eachModelFile in formModel.Files)
            {
                FileMetadata fileMetadata;
                if (filesInGroup.TryGetValue(eachModelFile.MetadataId, out fileMetadata))
                {
                    eachModelFile.BlobName = fileMetadata.BlobName;
                    eachModelFile.ContainerName = fileMetadata.ContainerName;
                    eachModelFile.ExpirationDate = fileMetadata.ExpirationDate;
                    eachModelFile.SourceFileName = fileMetadata.SourceFileName;
                }
            }
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            var patientFileGroup = this.db.PatientFileGroups
                .Include("PatientFiles")
                .Include("PatientFiles.FileMetadata")
                .First(m => m.Id == id);

            var patientFiles = patientFileGroup.PatientFiles.ToArray();

            var metadatas = patientFiles.Select(pf => pf.FileMetadata).ToArray();

            try
            {
                this.db.PatientFileGroups.DeleteObject(patientFileGroup);

                foreach (var patientFile in patientFiles)
                    this.db.PatientFiles.DeleteObject(patientFile);

                foreach (var metadata in metadatas)
                    TempFileController.DeleteFileByMetadata(metadata, this.db, this.storage);

                this.db.SaveChanges();

                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [SelfPermission]
        public ActionResult Image(int id, int w, int h)
        {
            var dbPatientFile = this.db.PatientFiles.Include("FileMetadata").FirstOrDefault(m => m.Id == id);

            DateTime modifiedSince;
            bool hasModifiedSince = DateTime.TryParse(this.Request.Headers.Get("If-Modified-Since"), out modifiedSince);

            // Images in the database never change after they are uploaded.
            // They can be deleted though.
            if (dbPatientFile == null)
                return new StatusCodeResult(HttpStatusCode.NotFound);

            // If the request suggests that the client has any previous version of the file,
            // then just indicate it hasn't changed, since these images never change.
            if (hasModifiedSince)
                return new StatusCodeResult(HttpStatusCode.NotModified);

            // Caching image forever.
            this.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
            this.HttpContext.Response.Cache.SetMaxAge(DebugConfig.IsDebug ? TimeSpan.FromMinutes(1) : TimeSpan.MaxValue);
            this.HttpContext.Response.Cache.SetLastModified(this.datetimeService.UtcNow);

            var metadata = dbPatientFile.FileMetadata;

            ActionResult result;
            try
            {
                result = metadata != null
                    ? this.GetOrCreateThumb(metadata, this.storage, this.datetimeService, w, h)
                    : new StatusCodeResult(HttpStatusCode.NotFound);
            }
            catch (OutOfMemoryException)
            {
                // this means that the image could not be generated because the image is too large
                result = this.Redirect(this.Url.Content("~/Content/Images/App/FileIcons/generic-outline-80.png"));
            }

            if (result is StatusCodeResult)
            {
                var statusResult = result as StatusCodeResult;

                //return this.Redirect(this.Server.MapPath("~/Content/Images/App/FileIcons/error-outline-80.png"));

                this.HttpContext.Response.StatusCode = (int)statusResult.StatusCode;
                var stream = System.IO.File.OpenRead(this.Server.MapPath("~/Content/Images/App/FileIcons/error-outline-80.png"));

                using (stream)
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return this.File(memoryStream.ToArray(), "image/png", metadata.SourceFileName);
                }
            }

            return result;
        }

        [SelfPermission]
        public ActionResult File(int id)
        {
            var dbPatientFile = this.db.PatientFiles.Include("FileMetadata").First(m => m.Id == id);

            DateTime modifiedSince;
            bool hasModifiedSince = DateTime.TryParse(this.Request.Headers.Get("If-Modified-Since"), out modifiedSince);

            // File in the database never change after they are uploaded.
            // They can be deleted though.
            if (dbPatientFile == null)
                return new StatusCodeResult(HttpStatusCode.NotFound);

            // If the request suggests that the client has any previous version of the file,
            // then just indicate it hasn't changed, since these images never change.
            if (hasModifiedSince)
                return new StatusCodeResult(HttpStatusCode.NotModified);

            // Caching file forever.
            this.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
            this.HttpContext.Response.Cache.SetMaxAge(DebugConfig.IsDebug ? TimeSpan.FromMinutes(1) : TimeSpan.MaxValue);
            this.HttpContext.Response.Cache.SetLastModified(this.datetimeService.UtcNow);

            var metadata = dbPatientFile.FileMetadata;

            if (metadata != null)
            {
                var fileName = metadata.SourceFileName;
                var stream = this.storage.DownloadFileFromStorage(metadata.ContainerName, metadata.BlobName);

                if (stream == null)
                    return new StatusCodeResult(HttpStatusCode.NotFound);

                using (stream)
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return this.File(memoryStream.ToArray(), MimeTypesHelper.GetContentType(fileName), fileName);
                }
            }

            return new StatusCodeResult(HttpStatusCode.NotFound);
        }
    }
}
