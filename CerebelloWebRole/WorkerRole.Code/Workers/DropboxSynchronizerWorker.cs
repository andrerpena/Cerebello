using System;
using System.Linq;
using System.Threading;
using CerebelloWebRole.Code.Access;
using CerebelloWebRole.Code.Helpers;
using DropNet;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    /// <summary>
    /// Sends e-mails to patients.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public class DropboxSynchronizerWorker : BaseCerebelloWorker
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

            try
            {
                using (var db = this.CreateNewCerebelloEntities())
                {
                    var dbWrapper = new CerebelloEntitiesAccessFilterWrapper(db);
                    var patientsToBackup = db.Patients.Where(p => p.Practice.DropboxInfos.Any() && !p.IsBackedUp).ToList();
                    foreach (var patient in patientsToBackup)
                    {
                        try
                        {
                            dbWrapper.SetCurrentUserById(patient.Doctor.Users.First().Id);
                            var patientBackup = BackupHelper.GeneratePatientBackup(dbWrapper, patient);
                            var dropboxInfo = patient.Practice.DropboxInfos.First();
                            var dropbox = new DropNetClient("r1ndpw0o5lh755x", "qrmdxee9kzbd81i", dropboxInfo.Token, dropboxInfo.Secret)
                            {
                                UseSandbox = true
                            };

                            var fileName = string.Format("{0} (id:{1})", patient.Person.FullName, patient.Id) + ".zip";
                            try
                            {
                                dropbox.Delete("/" + fileName);
                            }
                            catch
                            {

                            }
                            dropbox.UploadFile(
                                "/", fileName, patientBackup.ToArray());
                            patient.IsBackedUp = true;
                            dbWrapper.SaveChanges();
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            // setting locker value to 0
            if (Interlocked.Exchange(ref locker, 0) != 1)
                throw new Exception("The value of locker should be 1 before setting it to 0.");
        }

    }
}
