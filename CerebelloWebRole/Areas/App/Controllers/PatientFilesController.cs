using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Code.WindowsAzure;
using Ionic.Zip;
using JetBrains.Annotations;
using File = Cerebello.Model.File;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Manages patient files
    /// </summary>
    public class PatientFilesController : DoctorController
    {
        [SelfPermission]
        public ActionResult DownloadBackup(int patientId)
        {
            var patient = this.db.Patients.First(p => p.Id == patientId);
            var backup = BackupHelper.GeneratePatientBackup(this.db, patient);
            return this.File(
                backup,
                "application/zip",
                patient.Person.FullName + " - " + this.GetPracticeLocalNow().ToShortDateString() + ".zip");
        }

        /// <summary>
        /// Downloads the specified file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [SelfPermission]
        public ActionResult DownloadFile([NotNull] string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            var storageManager = new WindowsAzureBlobStorageManager();
            var fileStream = storageManager.DownloadFileFromStorage(Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, fileName);
            fileStream.Seek(0, SeekOrigin.Begin);
            var fileExtension = Path.GetExtension(fileName);
            var mimeType = MimeTypesHelper.GetContentType(fileExtension);

            return this.File(fileStream, mimeType, fileName);
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
                        Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, patientFile.File.FileName);

                    zip.AddEntry(patientFile.File.FileName, fileStream);
                }
                zip.Save(zipMemoryStream);
            }

            zipMemoryStream.Seek(0, SeekOrigin.Begin);
            return this.File(
                zipMemoryStream,
                "application/zip",
                patient.Person.FullName + " - Arquivos - " + this.GetPracticeLocalNow().ToShortDateString() + ".zip");
        }

        /// <summary>
        /// Downloads a zip file with all files from all patients
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
                                    Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, patientFile.File.FileName);

                                innerZip.AddEntry(patientFile.File.FileName, fileStream);
                            }
                            catch (Exception ex)
                            {
                                // in this case the file exists in the database but does not exist in the storage.
                                // LOG HERE
                            }
                        }
                        innerZip.Save(innerZipMemoryStream);
                    }
                    innerZipMemoryStream.Seek(0, SeekOrigin.Begin);
                    outerZip.AddEntry(patient.Person.FullName + ".zip", innerZipMemoryStream);
                }

                outerZip.Save(mainZipMemoryStream);
            }

            mainZipMemoryStream.Seek(0, SeekOrigin.Begin);
            return this.File(
                mainZipMemoryStream,
                "application/zip",
                this.Doctor.Users.ElementAt(0).Person.FullName + " - Arquivos dos pacientes - " +
                    this.GetPracticeLocalNow().ToShortDateString() + ".zip");
        }

        private static PatientFilesGroupViewModel GetViewModel(PatientFileGroup dbFileGroup, Func<DateTime, DateTime> toLocal)
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
                    FileContainer = dbFile.File.ContainerName,
                    FileName = dbFile.File.FileName,
                }));

            FillFileLengths(result, null);

            return result;
        }

        private static void FillFileLengths(PatientFilesGroupViewModel viewModel, int? dbUserId)
        {
            // reading file sizes from the storage
            foreach (var eachFile in viewModel.Files)
            {
                var isTemp = eachFile.FileContainer.StartsWith(
                    string.Format(
                        @"{0}\patient-files-{1}-",
                        Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME,
                        dbUserId));

                var isFinal = eachFile.FileContainer == Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME;

                // Validating each file location... otherwise this could be a security hole.
                if (!isTemp && !isFinal)
                    throw new Exception("Invalid file location.");

                var fileName = isTemp ?
                    string.Format(eachFile.FileName) :
                    string.Format("{0}{1}", eachFile.Id, Path.GetExtension(eachFile.FileName));

                var containerPath = eachFile.FileContainer;
                var fullFileName = Path.Combine(containerPath, fileName);
                eachFile.FileLength = FileHelper.GetFileLength(fullFileName);
            }
        }

        public static List<TempFileController.FilesStatus> GetFilesStatus(UrlHelper url, List<PatientFileViewModel> files, string prefix, int dbUserId)
        {
            return files.Select(f =>
                {
                    var location = f.FileContainer;
                    var fileName = f.FileName;
                    var fileStatus = new TempFileController.FilesStatus(fileName, f.FileLength, prefix, location, f.FileTitle, f.Id);

                    var isTemp = f.FileContainer.StartsWith(
                        string.Format(
                            @"{0}\patient-files-{1}-",
                            Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME,
                            dbUserId));

                    var isFinal = f.FileContainer == Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME;

                    // Validating each file location... otherwise this could be a security hole.
                    if (!isTemp && !isFinal)
                        throw new Exception("Invalid file location.");

                    var storageFileName = isTemp ? fileName : string.Format("{0}{1}", f.Id, Path.GetExtension(f.FileName));

                    bool imageThumbOk = false;
                    try
                    {
                        var thumbName = string.Format(@"thumbs-{0}x{1}\{2}", 80, 80, fileName);
                        byte[] array;
                        string contentType;
                        bool thumbExists = TryGetOrCreateThumb(80, 80, location, storageFileName, thumbName, true, out array, out contentType);
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
                        if (isTemp)
                            fileStatus.UrlLarge = url.Action("Thumb", "TempFile", new { w = 1024, h = 768, location, fileName });
                        else
                            fileStatus.UrlLarge = url.Action("Image", "PatientFiles", new { w = 1024, h = 768, id = f.Id });
                    }

                    if (isTemp)
                    {
                        var tempLocation = location.Substring(location.IndexOf('\\'));
                        fileStatus.UrlFull = url.Action("File", "TempFile", new { tempLocation, fileName });
                    }
                    else
                    {
                        fileStatus.UrlFull = url.Action("File", "PatientFiles", new { id = f.Id });
                    }

                    return fileStatus;
                }).ToList();
        }

        public ActionResult Details(int id)
        {
            var patientFileGroup = this.db.PatientFileGroups.First(pf => pf.Id == id);
            return this.View(GetViewModel(patientFileGroup, this.GetToLocalDateTimeConverter()));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(Dictionary<string, PatientFilesGroupViewModel> patientFilesGroups)
        {
            return this.Edit(patientFilesGroups);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            PatientFilesGroupViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel(
                    (from pf in this.db.PatientFileGroups where pf.Id == id select pf).First(),
                    this.GetToLocalDateTimeConverter());
            else
            {
                Debug.Assert(patientId != null, "patientId != null");
                viewModel = new PatientFilesGroupViewModel
                    {
                        Id = null,
                        PatientId = patientId.Value,
                        FileGroupDate = null,
                        ReceiveDate = this.GetPracticeLocalNow()
                    };
            }

            viewModel.NewGuid = Guid.NewGuid();

            return this.View("Edit", viewModel);
        }

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
                        CreatedOn = this.GetUtcNow(),
                    };

                this.db.PatientFileGroups.AddObject(dbFileGroup);
            }
            else
            {
                dbFileGroup = this.db.PatientFileGroups.FirstOrDefault(pe => pe.Id == formModel.Id);
            }

            Debug.Assert(dbFileGroup != null, "dbFileGroup != null");
            var allExistingFiles = dbFileGroup.PatientFiles.ToList();

            var locationsToKill = new HashSet<string>();
            var idsToKeep = new HashSet<int>(formModel.Files.Where(f => f.Id != null).Select(f => f.Id.Value));

            var storageActions = new List<Action>(formModel.Files.Count);

            foreach (var eachFile in formModel.Files)
            {
                var isTemp = eachFile.FileContainer.StartsWith(
                    string.Format(
                        @"{0}\patient-files-{1}-",
                        Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME,
                        this.DbUser.Id));

                var isFinal = eachFile.FileContainer == Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME;

                // Validating each file location... otherwise this could be a security hole.
                if (!isTemp && !isFinal)
                    throw new Exception("Invalid file location.");

                PatientFile patientFile;

                if (eachFile.Id == null)
                {
                    // creating and adding the new patient file
                    Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
                    patientFile = new PatientFile
                    {
                        File = new File
                        {
                            CreatedOn = this.GetUtcNow(),
                            PracticeId = this.DbUser.PracticeId,
                            FileName = eachFile.FileName,
                            ContainerName = Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME,
                        },
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };
                    dbFileGroup.PatientFiles.Add(patientFile);

                    // creating delegate to move the file in the storage, from the temporary location to the final location
                    if (isTemp)
                    {
                        var currentFile = eachFile;
                        var tempPath = currentFile.FileContainer;
                        var destPath = Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME;
                        var tempFullFileName = Path.Combine(tempPath, currentFile.FileName);

                        FileHelper.CreateDirectory(destPath);

                        locationsToKill.Add(tempPath);

                        Action moveToFinalLocation = () =>
                            {
                                // patientFile.Id will only be available after saving patientFile to the database
                                var fileName = patientFile.Id + Path.GetExtension(currentFile.FileName);
                                var destFullFileName = Path.Combine(destPath, fileName);
                                FileHelper.Move(tempFullFileName, destFullFileName);
                            };

                        storageActions.Add(moveToFinalLocation);
                    }
                }
                else
                {
                    patientFile = allExistingFiles.FirstOrDefault(pe => pe.Id == eachFile.Id);
                }

                Debug.Assert(patientFile != null, "patientFile != null");

                patientFile.Title = eachFile.FileTitle;
            }

            // deleting files that were removed
            foreach (var patientFile in allExistingFiles)
            {
                if (!idsToKeep.Contains(patientFile.Id))
                {
                    var originalFileName = patientFile.File.FileName;
                    var destPath = Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME;

                    this.db.Files.DeleteObject(patientFile.File);
                    this.db.PatientFiles.DeleteObject(patientFile);

                    Action removeFile = () =>
                    {
                        // patientFile.Id will only be available after saving patientFile to the database
                        var fileName = patientFile.Id + Path.GetExtension(originalFileName);
                        var destFullFileName = Path.Combine(destPath, fileName);
                        FileHelper.Delete(destFullFileName);
                    };

                    storageActions.Add(removeFile);
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

                patientFile.Patient.IsBackedUp = false;
                this.db.SaveChanges();

                // moving files that are stored in a temporary location
                foreach (var moveAction in storageActions)
                    moveAction();

                foreach (var location in locationsToKill)
                    FileHelper.DeleteDirectory(location);

                return this.View("Details", GetViewModel(dbFileGroup, this.GetToLocalDateTimeConverter()));
            }

            FillFileLengths(formModel, this.DbUser.Id);
            return this.View("Edit", formModel);
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            var patientFile = this.db.PatientFiles.First(m => m.Id == id);
            var file = patientFile.File;
            try
            {
                var storageManager = new WindowsAzureBlobStorageManager();
                storageManager.DeleteFileFromStorage(Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, patientFile.File.FileName);

                this.db.PatientFiles.DeleteObject(patientFile);
                this.db.Files.DeleteObject(file);
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
            var dbPatientFile = this.db.PatientFiles.First(m => m.Id == id);
            var dbFile = dbPatientFile.File;

            var container = dbFile.ContainerName;
            var fileName = string.Format("{0}{1}", dbPatientFile.Id, Path.GetExtension(dbFile.FileName));
            var thumbFileName = string.Format("{0}.{1}x{2}.png", dbPatientFile.Id, w, h);

            var result = this.GetOrCreateThumb(w, h, container, fileName, thumbFileName);
            if (result is StatusCodeResult)
            {
                var statusResult = result as StatusCodeResult;

                this.HttpContext.Response.StatusCode = (int)statusResult.StatusCode;
                var stream = System.IO.File.OpenRead(this.Server.MapPath("~/Content/Images/App/FileIcons/error-outline-80.png"));

                using (stream)
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return this.File(memoryStream.ToArray(), "image/png", dbFile.FileName);
                }
            }

            return result;
        }

        [SelfPermission]
        public ActionResult File(int id)
        {
            var dbPatientFile = this.db.PatientFiles.First(m => m.Id == id);
            var dbFile = dbPatientFile.File;

            var container = dbFile.ContainerName;
            var storageFileName = string.Format("{0}{1}", dbPatientFile.Id, Path.GetExtension(dbFile.FileName));

            var fullFileName = Path.Combine(container, storageFileName);

            var stream = FileHelper.OpenRead(fullFileName);
            if (stream == null)
            {
                this.HttpContext.Response.StatusCode = 404;
                stream = System.IO.File.OpenRead(this.Server.MapPath("~/Content/Images/App/FileIcons/error-outline-80.png"));
            }

            using (stream)
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return this.File(memoryStream.ToArray(), MimeTypesHelper.GetContentType(storageFileName), dbFile.FileName);
            }
        }
    }
}
