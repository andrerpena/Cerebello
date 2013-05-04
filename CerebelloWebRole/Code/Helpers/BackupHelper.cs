using System;
using System.IO;
using System.Linq;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Code.Access;
using CerebelloWebRole.Code.WindowsAzure;
using Ionic.Zip;
using JetBrains.Annotations;

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
                                Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, patientFile.File.FileName);
                            var fileName = patientFile.File.FileName;
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
    }

}