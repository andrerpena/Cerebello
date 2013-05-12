using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Cerebello.SolutionSetup.Parser;

namespace Cerebello.SolutionSetup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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
                        return;
                    }

                    sln = dirInfo.GetFiles("Cerebello.sln");
                }

                if (dirInfo != null)
                {
                    if (setupInfo.Props == null)
                        setupInfo.Props = new SetupInfo.Properties();

                    setupInfo.Props.SolutionPath = dirInfo.FullName;
                }
            }

            // check to see if setup is needed
            bool isSetupOk = setupInfo.FirestarterUncommitedConfig.IsGreaterOrEqual(1)
                || setupInfo.TestsUncommitedConfig.IsGreaterOrEqual(1)
                || setupInfo.FirestarterUncommitedConfig.IsGreaterOrEqual(1);

            if (isSetupOk)
                return;

            // check to see if solution is already setup
            if (!HasAdminPrivileges())
            {
                if (!TryRestartWithAdminPrivileges())
                    MessageBox.Show("Could not start with administrative privileges.");

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var frm = new Form1(setupInfo);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                // applying the setup
                RunSetup(setupInfo);

                // saving setup info file
                var ws = new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize };
                using (var reader = XmlWriter.Create("solution-setup-info.xml", ws))
                {
                    ser.Serialize(reader, setupInfo);
                }
            }
        }

        private static bool HasAdminPrivileges()
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

        private static bool TryRestartWithAdminPrivileges()
        {
            // Launch itself as administrator
            var proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas",
            };

            try
            {
                Process.Start(proc);
            }
            catch
            {
                // The user refused to allow privileges elevation.
                // Do nothing and return directly ...
                return false;
            }

            return true;
        }

        private static void RunSetup(SetupInfo setupInfo)
        {
            if (!(setupInfo.FirestarterUncommitedConfig.IsGreaterOrEqual(1)))
            {
                try
                {
                    var fsUncommitedConfigTemplate = Path.Combine(
                        setupInfo.Props.SolutionPath,
                        @"Cerebello.Firestarter\Uncommited.template.config");

                    var fsUncommitedConfig = Path.Combine(
                        setupInfo.Props.SolutionPath,
                        @"Cerebello.Firestarter\Uncommited.config");

                    var newText = ProcessTemplate(File.ReadAllText(fsUncommitedConfigTemplate), setupInfo.Props, true);
                    File.WriteAllText(fsUncommitedConfig, newText);

                    setupInfo.FirestarterUncommitedConfig = new SetupInfo.Version(1);
                }
                catch
                {
                }
            }

            if (!(setupInfo.TestsUncommitedConfig.IsGreaterOrEqual(1)))
            {
                try
                {
                    var testUncommitedConfigTemplate = Path.Combine(
                                         setupInfo.Props.SolutionPath,
                                         @"CerebelloWebRole.Tests\Uncommited.template.config");

                    var testUncommitedConfig = Path.Combine(
                            setupInfo.Props.SolutionPath,
                            @"CerebelloWebRole.Tests\Uncommited.config");

                    var newText = ProcessTemplate(File.ReadAllText(testUncommitedConfigTemplate), setupInfo.Props, true);
                    File.WriteAllText(testUncommitedConfig, newText);

                    setupInfo.TestsUncommitedConfig = new SetupInfo.Version(1);
                }
                catch
                {
                }
            }

            var azureTarget =
                @"C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v11.0\Windows Azure Tools\1.8\Microsoft.WindowsAzure.targets";

            if (!(setupInfo.WindowsAzureTargets.IsGreaterOrEqual(1)))
            {
                try
                {
                    if (File.Exists(azureTarget))
                    {
                        var dllsToLetIn = new[] {
                            "System.EnterpriseServices.Wrapper.dll",
                        };

                        var strDlls = string.Join(
                            " And ",
                            dllsToLetIn.Select(s => string.Format("'%(WebFiles.Filename)%(WebFiles.Extension)' != '{0}'", s)));

                        var lines = File.ReadAllLines(azureTarget);
                        for (int itLine = 0; itLine < lines.Length; itLine++)
                        {
                            var curLine = lines[itLine];
                            if (curLine.Contains("<_AssembliesToValidate") && curLine.Contains("Include=\"@(WebFiles);@(WorkerFiles)\""))
                                lines[itLine] = "      <_AssembliesToValidate Include=\"@(WebFiles);@(WorkerFiles)\" "
                                    + "Condition=\" '%(Extension)' == '.dll'"
                                    + " And " + strDlls
                                    + "\" >";
                        }

                        File.WriteAllLines(azureTarget, lines);

                        setupInfo.WindowsAzureTargets = new SetupInfo.Version(1);
                    }
                }
                catch
                {
                }
            }
        }

        private static string ProcessTemplate(string template, object data, bool htmlEncode)
        {
            var result = Regex.Replace(template, @"{{(.*?)}}", m => MatchEval(m, data, htmlEncode));
            return result;
        }

        private static string MatchEval(Match m, object data, bool htmlEncode)
        {
            var parser = new SimpleParser(m.Groups[1].Value) { GlobalType = data.GetType() };
            var valueExecutor = parser.Read<TemplateParser.ValueBuilder, TemplateParser.INode>();
            var type = valueExecutor.Compile(parser.GlobalType);
            var value = valueExecutor.Execute(data);
            var result = value.ToString();
            if (htmlEncode)
                result = HttpUtility.HtmlEncode(result);

            return result;
        }
    }
}
