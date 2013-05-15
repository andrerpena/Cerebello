using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Code.Access;
using CerebelloWebRole.Code.Google;
using CerebelloWebRole.Code.WindowsAzure;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Ionic.Zip;
using JetBrains.Annotations;
using File = Google.Apis.Drive.v2.Data.File;

namespace CerebelloWebRole.Code.Helpers
{
    public static class BackupHelper
    {
        public static MemoryStream GeneratePatientBackup([NotNull] CerebelloEntitiesAccessFilterWrapper db,
                                                         [NotNull] Patient patient)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (patient == null) throw new ArgumentNullException("patient");

            var zipMemoryStream = new MemoryStream();
            using (var patientZip = new ZipFile())
            {
                var storageManager = new WindowsAzureBlobStorageManager();

                // add the patient history as pdf
                var pdf = ReportController.ExportPatientsPdf(patient.Id, db, patient.Practice, patient.Doctor);
                patientZip.AddEntry(string.Format("{0} - Histórico.pdf", patient.Person.FullName), pdf.DocumentBytes);

                // if the person has a picture, add it to the backup
                if (patient.Person.PictureBlobName != null)
                {
                    var picture = storageManager.DownloadFileFromStorage(Constants.PERSON_PROFILE_PICTURE_CONTAINER_NAME, patient.Person.PictureBlobName);
                    patientZip.AddEntry(string.Format("{0} - Perfil.png", patient.Person.FullName), picture);
                }

                // if the person has files, add them to the backup
                var patientFiles = db.PatientFiles.Where(pf => pf.PatientId == patient.Id).ToList();
                if (patientFiles.Any())
                {
                    using (var patientFilesZip = new ZipFile())
                    {
                        var patientFilesZipMemoryStream = new MemoryStream();
                        foreach (var patientFile in patientFiles)
                        {
                            var fileStream = storageManager.DownloadFileFromStorage(
                                Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, patientFile.FileMetadata.FileName);
                            var fileName = patientFile.FileMetadata.FileName;
                            for (var i = 2;; i++)
                            {
                                if (patientFilesZip.ContainsEntry(fileName))
                                {
                                    fileName = Path.GetFileNameWithoutExtension(fileName) + " " + i + Path.GetExtension(fileName);
                                }
                                    
                                else
                                {
                                    break;
                                }
                            }
                            patientFilesZip.AddEntry(fileName, fileStream);
                        }

                        patientFilesZip.Save(patientFilesZipMemoryStream);
                        patientFilesZipMemoryStream.Seek(0, SeekOrigin.Begin);
                        patientZip.AddEntry(string.Format("{0} - Arquivos.zip", patient.Person.FullName), patientFilesZipMemoryStream);
                    }
                }

                patientZip.Save(zipMemoryStream);
            }

            zipMemoryStream.Seek(0, SeekOrigin.Begin);
            return zipMemoryStream;
        }

        public static void BackupEverything([NotNull] CerebelloEntities db, List<string> errors = null)
        {
            if (db == null) throw new ArgumentNullException("db");
            if(errors == null)
                errors =new List<string>();

            using (db)
            {
                var dbWrapper = new CerebelloEntitiesAccessFilterWrapper(db);
                var patientsToBackup = db.Patients.Where(p => p.Doctor.Users.Any(u => u.Person.GoogleUserAccoutInfoes.Any()) && !p.IsBackedUp).GroupBy(p => p.DoctorId);
                foreach (var patientGroup in patientsToBackup.ToList())
                {
                    try
                    {
                        var doctor = db.Doctors.First(d => d.Id == patientGroup.Key);
                        dbWrapper.SetCurrentUserById(doctor.Users.First().Id);
                        var doctorGoogleAccountInfo = doctor.Users.First().Person.GoogleUserAccoutInfoes.FirstOrDefault();
                        if (doctorGoogleAccountInfo != null)
                        {
                            // in this case the doctor for these patients have a Google Account associated
                            var requestAccessResult = GoogleApiHelper.RequestAccessToken(doctorGoogleAccountInfo.RefreshToken);
                            var authenticator = GoogleApiHelper.GetAuthenticator(
                                doctorGoogleAccountInfo.RefreshToken, requestAccessResult.access_token);
                            var driveService = new DriveService(authenticator);

                            // create Cerebello folder if it does not exist
                            var practiceGoogleDriveInfo = doctor.Practice.GoogleDrivePracticeInfoes.FirstOrDefault();
                            File cerebelloFolder = null;
                            if (practiceGoogleDriveInfo != null)
                            {
                                try
                                {
                                    cerebelloFolder = GoogleApiHelper.GetFile(driveService, practiceGoogleDriveInfo.CerebelloFolderId);
                                    if (cerebelloFolder.Labels.Trashed.HasValue && cerebelloFolder.Labels.Trashed.Value)
                                        cerebelloFolder = null;
                                }
                                catch (Exception ex)
                                {
                                    var errorMessage = "Error downloading file from Google Drive. Exception message: " + ex.Message;
                                    // the fucking user deleted the fucking folder OR something went wrong downloading the file
                                    Trace.TraceError(errorMessage);
                                    errors.Add(errorMessage);
                                }
                            }
                            if (cerebelloFolder == null)
                            {
                                cerebelloFolder = GoogleApiHelper.CreateFolder(driveService, "Cerebello", "Pasta do Cerebello");
                                if (practiceGoogleDriveInfo != null)
                                    practiceGoogleDriveInfo.CerebelloFolderId = cerebelloFolder.Id;
                                else
                                {
                                    practiceGoogleDriveInfo = new GoogleDrivePracticeInfo()
                                    {
                                        CerebelloFolderId = cerebelloFolder.Id,
                                        PracticeId = doctor.PracticeId
                                    };
                                    doctor.Practice.GoogleDrivePracticeInfoes.Add(practiceGoogleDriveInfo);
                                }
                                db.SaveChanges();
                            }

                            foreach (var patient in patientGroup)
                            {
                                try
                                {
                                    var patientBackup = BackupHelper.GeneratePatientBackup(dbWrapper, patient);
                                    var fileName = string.Format("{0} (id:{1})", patient.Person.FullName, patient.Id) + ".zip";
                                    var fileDescription = string.Format(
                                        "Arquivo de backup do(a) paciente {0} (id:{1})", patient.Person.FullName, patient.Id);

                                    // check if the file exists already
                                    var patientGoogleDriveFile = patient.GoogleDrivePatientInfoes.FirstOrDefault();
                                    File googleDrivePatientFile = null;
                                    if (patientGoogleDriveFile != null)
                                    {
                                        try
                                        {
                                            // get reference to existing file to make sure it exists
                                            var existingFile = GoogleApiHelper.GetFile(
                                                driveService, patientGoogleDriveFile.PatientBackupFileId);
                                            if (!existingFile.Labels.Trashed.HasValue || !existingFile.Labels.Trashed.Value)
                                            {
                                                googleDrivePatientFile = GoogleApiHelper.UpdateFile(
                                                    driveService,
                                                    patientGoogleDriveFile.PatientBackupFileId,
                                                    fileName,
                                                    fileDescription,
                                                    MimeTypesHelper.GetContentType(".zip"),
                                                    patientBackup);
                                                if (googleDrivePatientFile.Labels.Trashed.HasValue && googleDrivePatientFile.Labels.Trashed.Value)
                                                    googleDrivePatientFile = null;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            var errorMessage = "Error updating file from Google Drive. Exception message: " + ex.Message;
                                            // the fucking user deleted the fucking folder OR something went wrong downloading the file
                                            Trace.TraceError(errorMessage);
                                            errors.Add(errorMessage);
                                        }
                                    }

                                    if (googleDrivePatientFile == null)
                                    {
                                        googleDrivePatientFile = GoogleApiHelper.CreateFile(
                                            driveService,
                                            fileName,
                                            fileDescription,
                                            MimeTypesHelper.GetContentType(".zip"),
                                            patientBackup,
                                            new List<ParentReference>() { new ParentReference() { Id = cerebelloFolder.Id } });

                                        if (patientGoogleDriveFile != null)
                                            patientGoogleDriveFile.PatientBackupFileId = googleDrivePatientFile.Id;
                                        else
                                        {
                                            patient.GoogleDrivePatientInfoes.Add(
                                                new GoogleDrivePatientInfo()
                                                {
                                                    PatientBackupFileId = googleDrivePatientFile.Id,
                                                    PracticeId = doctor.PracticeId
                                                });
                                        }
                                    }
                                    patient.IsBackedUp = true;
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    var errorMessage = "Error synchronizing files for a specific doctor. Exception message" + ex.Message;
                                    // the fucking user deleted the fucking folder OR something went wrong downloading the file
                                    Trace.TraceError(errorMessage);
                                    errors.Add(errorMessage);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = "Error synchronizing files. Exception message" + ex.Message;
                        // the fucking user deleted the fucking folder OR something went wrong downloading the file
                        Trace.TraceError(errorMessage);
                        errors.Add(errorMessage);
                    }
                }
            }
        }
    }

}