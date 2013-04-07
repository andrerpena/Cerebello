using System;
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
            var patient = this.db.Patients.FirstOrDefault(p => p.Id == patientId);
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
            return this.File(zipMemoryStream, "application/zip", patient.Person.FullName + " - Arquivos - " + ConvertToLocalDateTime(this.DbPractice, this.GetUtcNow()).ToShortDateString() + ".zip");
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
            return this.File(mainZipMemoryStream, "application/zip", this.Doctor.Users.ElementAt(0).Person.FullName + " - Arquivos dos pacientes - " + ConvertToLocalDateTime(this.DbPractice, this.GetUtcNow()).ToShortDateString() + ".zip");
        }

        private static PatientFileViewModel GetViewModel(PatientFile patientFile)
        {
            if (patientFile == null)
                return new PatientFileViewModel();
            return new PatientFileViewModel()
                {
                    Id = patientFile.Id,
                    PatientId = patientFile.PatientId,
                    CreatedOn = patientFile.File.CreatedOn,
                    Description = patientFile.File.Description,
                    FileContainer = patientFile.File.ContainerName,
                    FileName = patientFile.File.FileName
                };
        }

        public ActionResult Details(int id)
        {
            var patientFile = this.db.PatientFiles.First(pf => pf.Id == id);
            return this.View(GetViewModel(patientFile));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(PatientFileViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            PatientFileViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel((from pf in db.PatientFiles where pf.Id == id select pf).First());
            else
            {
                Debug.Assert(patientId != null, "patientId != null");
                viewModel = new PatientFileViewModel()
                    {
                        Id = null,
                        PatientId = patientId.Value
                    };
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(PatientFileViewModel formModel)
        {
            var fileName = Request.Headers["X-File-Name"];

            // if fileSize is 0, fileName will not be null. But I must ensure it's not null
            // otherwise I won't be able to store the file
            if (!formModel.Id.HasValue && String.IsNullOrEmpty(fileName))
                this.ModelState.AddModelError<PatientFileViewModel>(model => model.PostedFile, "O arquivo é requerido");

            PatientFile patientFile;

            if (formModel.Id == null)
            {
                Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
                patientFile = new PatientFile()
                {
                    File = new File()
                        {
                            CreatedOn = this.GetUtcNow(),
                            PracticeId = this.DbUser.PracticeId
                        },
                    PatientId = formModel.PatientId.Value,
                    PracticeId = this.DbUser.PracticeId
                };
                this.db.PatientFiles.AddObject(patientFile);
            }
            else
                patientFile = this.db.PatientFiles.FirstOrDefault(pe => pe.Id == formModel.Id);

            if (this.ModelState.IsValid)
            {
                // when the file is new, aditional things must be done, like uploading the 
                // file to Azure
                if (!formModel.Id.HasValue)
                {
                    //File's content is available in Request.InputStream property
                    var fileContent = Request.InputStream;
                    //Creating a FileStream to save file's content
                    fileContent.Seek(0, SeekOrigin.Begin);

                    var storageManager = new WindowsAzureBlobStorageManager();
                    storageManager.UploadFileToStorage(fileContent, Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, fileName);
                    fileContent.Dispose();

                    Debug.Assert(fileName != null, "fileName != null");

                    patientFile.File.FileName = fileName;
                    patientFile.File.ContainerName = Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME;
                }

                Debug.Assert(patientFile != null, "patientFile != null");

                patientFile.File.Description = formModel.Description;
                this.db.SaveChanges();
                return View("Details", GetViewModel(patientFile));
            }

            return this.View("Edit", GetViewModel(patientFile));
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

    }
}