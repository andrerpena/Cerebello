using System;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.WorkerRole.Code.Workers;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CerebelloWebRole
{
    public class WebRole : RoleEntryPoint
    {
        // Running Multiple Websites in a Windows Azure Web Role 34:
        // http://www.wadewegner.com/2011/02/running-multiple-websites-in-a-windows-azure-web-role/

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        public override void Run()
        {
            RouteHelper.RegisterAllRoutes();

            // Creating workers and running them.
            var intervalScheduler30Min = new IntervalWorkerScheduler(TimeSpan.FromMinutes(30.0))
                {
                    new EmailSenderWorker()
                };

            intervalScheduler30Min.Start();

            // Calling base to stop execution of this method forever.
            base.Run();
        }
    }
}
