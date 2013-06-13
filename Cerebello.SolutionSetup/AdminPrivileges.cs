using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace Cerebello.SolutionSetup
{
    class AdminPrivileges
    {
        public static bool HasAdminPrivileges()
        {
            var identity = WindowsIdentity.GetCurrent();
            if (identity != null)
            {
                var principal = new WindowsPrincipal(identity);
                bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return isElevated;
            }

            return false;
        }

        public static Process TryRestartWithAdminPrivileges(string[] args = null)
        {
            args = args ?? new string[0];

            // Launch itself as administrator
            var proc = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas",
                    Arguments = String.Join(" ", args),
                };

            try
            {
                return Process.Start(proc);
            }
            catch
            {
                // The user refused to allow privileges elevation.
                // Do nothing and return directly ...
                return null;
            }
        }
    }
}