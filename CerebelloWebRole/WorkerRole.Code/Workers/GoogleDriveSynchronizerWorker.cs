using System;
using System.Diagnostics;
using System.Threading;
using CerebelloWebRole.Code.Helpers;

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

            Trace.TraceInformation("Google Drive service in execution (GoogleDriveSynchronizerWorker)");

            using (var db = this.CreateNewCerebelloEntities())
                BackupHelper.BackupEverything(db);

            // setting locker value to 0
            if (Interlocked.Exchange(ref locker, 0) != 1)
                throw new Exception("The value of locker should be 1 before setting it to 0.");
        }
    }
}
