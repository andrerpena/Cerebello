using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Cerebello.SolutionSetup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                return Main2(args);
            }
            catch
            {
            }

            return 99;
        }

        static int Main2(string[] args)
        {
            var joinArgs = string.Join(" ", args);

            var argsMatches = Regex.Matches(joinArgs, @"  (?<CHECK>\\c\w|\\check\w)  |  (?:\\d:""(?<DESKTOP>.*?)"")  |  (?<DEBUG>\\d\w|\\debug\w)  ", RegexOptions.IgnorePatternWhitespace);

            bool debug = argsMatches.Cast<Match>().Any(m => m.Groups["DEBUG"].Success);

            if (debug)
                Debugger.Launch();

            bool check = argsMatches.Cast<Match>().Any(m => m.Groups["CHECK"].Success);
            string desktopPath = argsMatches.Cast<Match>()
                .Select(m => m.Groups["DESKTOP"])
                .Where(m => m.Success)
                .Select(m => m.Value)
                .LastOrDefault();

            if (string.IsNullOrWhiteSpace(desktopPath))
                desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            var newArgs = new List<string>();
            if (check) newArgs.Add(@"\c");
            if (!string.IsNullOrWhiteSpace(desktopPath))
                newArgs.Add(string.Format(@"\d:""{0}""", desktopPath));

            SetupInfo setupInfo;
            var ser = new XmlSerializer(typeof(SetupInfo));

            // Loading setup info file
            if (File.Exists("solution-setup-info.xml"))
            {
                using (var reader = XmlReader.Create("solution-setup-info.xml"))
                {
                    setupInfo = (SetupInfo)ser.Deserialize(reader);
                }
            }
            else
            {
                setupInfo = new SetupInfo();
            }

            // set values in the setup info
            {
                var dirInfo = new DirectoryInfo(Environment.CurrentDirectory);
                var sln = dirInfo.GetFiles("Cerebello.sln");
                while (!sln.Any())
                {
                    dirInfo = dirInfo.Parent;
                    if (dirInfo == null)
                    {
                        MessageBox.Show("Cannot find 'Cerebello.sln' in the current directory or any parent directory.");
                        return 2;
                    }

                    sln = dirInfo.GetFiles("Cerebello.sln");
                }

                if (dirInfo != null)
                {
                    if (setupInfo.Props == null)
                        setupInfo.Props = new SetupInfo.Properties();

                    setupInfo.Props.SolutionPath = dirInfo.FullName;
                }

                setupInfo.Props.DesktopPath = desktopPath;
            }

            // check to see if setup is needed
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            bool isSetupOk = setupInfo.Version == currentVersion;

            if (check && isSetupOk)
                return 0;

            setupInfo.Version = currentVersion;

            // check to see if solution is already setup
            if (!AdminPrivileges.HasAdminPrivileges())
            {
                if (Debugger.IsAttached)
                    newArgs.Add(@"\debug");

                var proc = AdminPrivileges.TryRestartWithAdminPrivileges(newArgs.ToArray());
                if (proc == null)
                {
                    MessageBox.Show("Could not start solution setup with administrative privileges.");
                    return 3;
                }

                proc.WaitForExit();

                return proc.ExitCode;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var frm = new Form1(setupInfo);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                var dirVisualStudio = new DirectoryInfo(@"C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio");
                var azureTargets = dirVisualStudio.GetFiles("Microsoft.WindowsAzure.targets", SearchOption.AllDirectories);
                setupInfo.AzureTargets = azureTargets;

                // applying the setup
                RunSetup(setupInfo);

                // saving setup info file
                var ws = new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize };
                using (var reader = XmlWriter.Create("solution-setup-info.xml", ws))
                {
                    ser.Serialize(reader, setupInfo);
                }
            }
            else
            {
                return 1;
            }

            return 0;
        }

        private static void RunSetup(SetupInfo setupInfo)
        {
            var listSteps = new List<Action<SetupInfo>>
                {
                    SetupSteps.UncommitedXml,
                    SetupSteps.FirestarterUncommitedConfig,
                    SetupSteps.TestUncommitedConfig,
                    SetupSteps.WindowsAzureTargets,
                };

            foreach (var step in listSteps)
            {
                try
                {
                    step(setupInfo);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }
        }
    }
}
