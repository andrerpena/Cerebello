using System;
using System.Diagnostics;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.WorkerRole.Code.Workers;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CerebelloWebRole
{
    public class WebRole : RoleEntryPoint
    {
        public override void Run()
        {
            RouteHelper.RegisterAllRoutes();

            var config = DiagnosticMonitor.GetDefaultInitialConfiguration();
            // Set an overall quota of 8GB.
            config.OverallQuotaInMB = 4096;
            // Set the sub-quotas and make sure it is less than the OverallQuotaInMB set above
            config.Logs.BufferQuotaInMB = 512;

            var myTimeSpan = TimeSpan.FromMinutes(2);
            config.Logs.ScheduledTransferPeriod = myTimeSpan;//Transfer data to storage every 2 minutes

            // Filter what will be sent to persistent storage.
            config.Logs.ScheduledTransferLogLevelFilter = LogLevel.Undefined;//Transfer everything
            // Apply the updated configuration to the diagnostic monitor.
            // The first parameter is for the connection string configuration setting.
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", config);
            Trace.Listeners.Add(new DiagnosticMonitorTraceListener());
            MvcApplication.RegisterTraceListeners(Trace.Listeners);


            // Creating workers and running them.
            var testInfraScheduler = new IntervalWorkerScheduler(TimeSpan.FromMinutes(30))
                {
                    new TestWorker()
                };
            testInfraScheduler.Start();

            var emailSenderScheduler = new IntervalWorkerScheduler(TimeSpan.FromMinutes(30))
                {
                    new EmailSenderWorker()
                };
            emailSenderScheduler.Start();

            var googleDriverSynchronizerScheduler = new IntervalWorkerScheduler(TimeSpan.FromMinutes(60))
                {
                    new GoogleDriveSynchronizerWorker()
                };
            googleDriverSynchronizerScheduler.Start();

            // Calling base to hold execution in this method forever.
            base.Run();
        }
    }
}
