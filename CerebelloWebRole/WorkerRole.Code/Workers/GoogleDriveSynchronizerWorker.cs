using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cerebello.Model;
using CerebelloWebRole.Code.Access;
using CerebelloWebRole.Code.Google;
using CerebelloWebRole.Code.Helpers;
using Google.Apis.Drive.v2.Data;
using File = Google.Apis.Drive.v2.Data.File;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    /// <summary>
    /// Sends e-mails to patients.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public class GoogleDriveSynchronizerWorker : BaseCerebelloWorker
    // ReSharper restore UnusedMember.Global
    {
        private static int locker;

        /// <summary>
        /// Runs the worker once to send e-mails.
        /// </summary>
        public override void RunOnce()
        {
            // If this method is already running, then leave the already running alone, and return.
            // If it is not running, set the value os locker to 1 to indicate that now it is running.
            if (Interlocked.Exchange(ref locker, 1) != 0)
                return;

            using (var db = this.CreateNewCerebelloEntities())
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
                            var driveService = new Google.Apis.Drive.v2.DriveService(authenticator);

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
                                catch
                                {
                                    // the fucking user deleted the fucking folder. Don't worry. It will be created again below.
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
                                                if (googleDrivePatientFile.ExplicitlyTrashed.Value)
                                                    googleDrivePatientFile = null;
                                            }
                                        }
                                        catch
                                        {
                                            // the fucking user deleted the fucking file. Don't worry. It will be created again below.
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
                                catch
                                {

                                }
                            }
                        }
                    }
                    catch
                    {
                        // backup for a group of patients failed.
                        // todo: add log here
                    }
                }
            }

            // setting locker value to 0
            if (Interlocked.Exchange(ref locker, 0) != 1)
                throw new Exception("The value of locker should be 1 before setting it to 0.");
        }
    }
}
