using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;
using Cerebello.Model;
using CerebelloWebRole.Code.WindowsAzure;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    /// <summary>
    /// Tests worker infrastructure by 
    /// saving data to the database, 
    /// saving a test files in the storage, 
    /// and sending an e-mail to 'cerebello@cerebello.com.br'.
    /// </summary>
    public class TestWorker : BaseCerebelloWorker
    {
        private static int locker;

        /// <summary>
        /// Runs the worker once to do the tests.
        /// </summary>
        public override void RunOnce()
        {
            // If this method is already running, then leave the already running alone, and return.
            // If it is not running, set the value os locker to 1 to indicate that now it is running.
            if (Interlocked.Exchange(ref locker, 1) != 0)
                return;

            Trace.TraceInformation("Test worker service in execution (TestWorker)");

            var utcNow = this.GetUtcNow();
            using (var db = this.CreateNewCerebelloEntities())
            {
                // trying to save to the database
                Exception exSaveToDb = null;
                SPECIAL_Test dbObj = null;
                try
                {
                    var o = new SPECIAL_Test
                        {
                            CreatedOn = utcNow,
                        };

                    db.SPECIAL_Test.AddObject(o);

                    db.SaveChanges();

                    dbObj = o;
                }
                catch (Exception ex)
                {
                    exSaveToDb = ex;
                }

                if (exSaveToDb == null)
                    Trace.TraceInformation("Test worker: DB object saved");

                // trying to save a file in the storage
                Exception exSaveToStorage = null;
                try
                {
                    var storageManager = new WindowsAzureBlobStorageManager();
                    using (var stream = new MemoryStream(new byte[0]))
                        storageManager.UploadFileToStorage(
                            stream, "worker-test", string.Format("{0}", utcNow.ToString("yyyy'-'MM'-'dd hh'-'mm")));
                }
                catch (Exception ex)
                {
                    exSaveToStorage = ex;
                }

                if (exSaveToStorage == null)
                    Trace.TraceInformation("Test worker: blob saved to storage");

                // Sending e-mail about test status
                Exception exSendEmail = null;
                try
                {
                    var obj = new Dictionary<string, Exception>
                        {
                            { "exSaveToDb", exSaveToDb },
                            { "exSaveToStorage", exSaveToStorage },
                        };

                    var mailMessage = this.CreateEmailMessage("TestEmail", new MailAddress("cerebello@cerebello.com.br"), obj);
                    if (!this.TrySendEmail(mailMessage))
                        throw new Exception("Cannot send e-mail message.");
                }
                catch (Exception ex)
                {
                    exSendEmail = ex;
                }

                if (exSendEmail == null)
                    Trace.TraceInformation("Test worker: e-mail message sent");

                // Save result to storage
                var fileText = new StringBuilder(1000);
                fileText.AppendLine("File saved from TestWorker");

                if (exSaveToDb != null)
                {
                    fileText.AppendLine();
                    fileText.AppendLine("Save to DB failed: " + exSaveToDb.Message);
                }

                if (exSaveToStorage != null)
                {
                    fileText.AppendLine();
                    fileText.AppendLine("Save to Storage failed: " + exSaveToStorage.Message);
                }

                if (exSendEmail != null)
                {
                    fileText.AppendLine();
                    fileText.AppendLine("Send e-mail failed: " + exSendEmail.Message);
                }

                if (exSaveToStorage == null)
                    try
                    {
                        var storageManager = new WindowsAzureBlobStorageManager();
                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileText.ToString())))
                            storageManager.UploadFileToStorage(
                                stream, "worker-test", string.Format("{0}", utcNow.ToString("yyyy'-'MM'-'dd hh'-'mm")));
                    }
                    catch
                    {
                    }

                // Save result to db
                if (exSaveToDb == null)
                    try
                    {
                        if (dbObj != null)
                        {
                            dbObj.Value = fileText.ToString();
                            db.SaveChanges();
                        }
                    }
                    catch
                    {
                    }
            }

            // setting locker value to 0
            if (Interlocked.Exchange(ref locker, 0) != 1)
                throw new Exception("The value of locker should be 1 before setting it to 0.");
        }

    }
}
