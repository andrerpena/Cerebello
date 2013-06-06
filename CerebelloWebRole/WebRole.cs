using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.WorkerRole.Code.Workers;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CerebelloWebRole
{
    public class WebRole : RoleEntryPoint
    {
        /// <summary>
        /// Called by Windows Azure to initialize the role instance.
        /// </summary>
        /// <returns>
        /// True if initialization succeeds, False if it fails. The default implementation returns True.
        /// </returns>
        /// <remarks>
        /// <para> Override the OnStart method to run initialization code for your role. </para>
        /// <para> Before the OnStart method returns, the instance's status is set to Busy and the instance is not available
        ///             for requests via the load balancer. </para>
        /// <para> If the OnStart method returns false, the instance is immediately stopped. If the method
        ///             returns true, then Windows Azure starts the role by calling the 
        ///             <see cref="M:Microsoft.WindowsAzure.ServiceRuntime.RoleEntryPoint.Run"/> method. </para>
        /// <para> A web role can include initialization code in the ASP.NET Application_Start method instead of the OnStart method.
        ///             Application_Start is called after the OnStart method. </para>
        /// <para> Any exception that occurs within the OnStart method is an unhandled exception. </para>
        /// </remarks>
        public override bool OnStart()
        {
            // Trace listeners should always be the first thing here.
            MvcApplication.RegisterTraceListeners(Trace.Listeners);

            Trace.TraceInformation(string.Format("WebRole.OnStart(): webrole started! [Debug={0}]", DebugConfig.IsDebug));

            // This is where DLL's and other dependencies should be installed in the System.
            // Of course, a check must be made before installing anything to see if they are not yet installed.
            return base.OnStart();
        }

        /// <summary>
        /// Called by Windows Azure after the role instance has been initialized. This method serves as the
        ///             main thread of execution for your role.
        /// </summary>
        /// <remarks>
        /// <para> Override the Run method to implement your own code to manage the role's execution. The Run method should implement
        ///             a long-running thread that carries out operations for the role. The default implementation sleeps for an infinite
        ///             period, blocking return indefinitely. </para>
        /// <para> The role recycles when the Run method returns. </para>
        /// <para> Any exception that occurs within the Run method is an unhandled exception. </para>
        /// </remarks>
        public override void Run()
        {
            Trace.TraceInformation("WebRole.Run(): webrole running");

            var allConnections = string.Join(
                "; ",
                ConfigurationManager.ConnectionStrings
                    .OfType<ConnectionStringSettings>()
                    .Select(c => string.Format("{0}", c.Name)));

            Trace.TraceInformation("WebRole.Run(): Available connection strings: " + allConnections);
            Trace.TraceInformation("WebRole.Run(): DebugConfig.DataBaseConnectionString=" + DebugConfig.DataBaseConnectionString);

            RouteHelper.RegisterAllRoutes();

            // Creating workers and running them.
            var testInfraScheduler = new IntervalWorkerScheduler(TimeSpan.FromMinutes(30))
                {
                    new TestWorker()
                };
            testInfraScheduler.Start();
            Trace.TraceInformation("WebRole.Run(): TestWorker thread started");

            var emailSenderScheduler = new IntervalWorkerScheduler(TimeSpan.FromMinutes(30))
                {
                    new EmailSenderWorker()
                };
            emailSenderScheduler.Start();
            Trace.TraceInformation("WebRole.Run(): EmailSenderWorker thread started");

            var googleDriverSynchronizerScheduler = new IntervalWorkerScheduler(TimeSpan.FromMinutes(60))
                {
                    new GoogleDriveSynchronizerWorker()
                };
            googleDriverSynchronizerScheduler.Start();
            Trace.TraceInformation("WebRole.Run(): GoogleDriveSynchronizerWorker thread started");

            // Calling base to hold execution in this method forever.
            Trace.TraceInformation("WebRole.Run(): base.Run() -- running forever!");
            base.Run();
        }
    }
}
