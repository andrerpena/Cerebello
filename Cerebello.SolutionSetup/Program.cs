using System;
using System.Collections.Generic;
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

            var argsMatches = Regex.Matches(joinArgs, @"(?<CHECK>\\c\w|\\check\w)|(?:\\d:""(?<DESKTOP>.*?)"")");

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

                    setupInfo.Props.DesktopPath = desktopPath;
                    setupInfo.Props.SolutionPath = dirInfo.FullName;
                }
            }

            // check to see if setup is needed
            bool isSetupOk = setupInfo.FirestarterUncommitedConfig.IsGreaterOrEqual(1)
                || setupInfo.TestsUncommitedConfig.IsGreaterOrEqual(1)
                || setupInfo.FirestarterUncommitedConfig.IsGreaterOrEqual(1);

            if (check && isSetupOk)
                return 0;

            // check to see if solution is already setup
            if (!HasAdminPrivileges())
            {
                if (!TryRestartWithAdminPrivileges(newArgs.ToArray()))
                    MessageBox.Show("Could not start solution setup with administrative privileges.");

                return 3;
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
                RunSetup(setupInfo, !check);

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

        private static bool TryRestartWithAdminPrivileges(string[] args = null)
        {
            args = args ?? new string[0];

            // Launch itself as administrator
            var proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas",
                Arguments = string.Join(" ", args),
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

        private static void RunSetup(SetupInfo setupInfo, bool force)
        {
            if (force || !setupInfo.CerebelloUncommitedXml.IsGreaterOrEqual(1))
            {
                try
                {
                    var fsUncommitedXmlTemplate = Path.Combine(
                        setupInfo.Props.SolutionPath,
                        @"CerebelloWebRole\Uncommited.template.xml");

                    var fsUncommitedXml = Path.Combine(
                        setupInfo.Props.SolutionPath,
                        @"CerebelloWebRole\Uncommited.xml");

                    var newText = ProcessTemplate(File.ReadAllText(fsUncommitedXmlTemplate), setupInfo.Props, true);
                    File.WriteAllText(fsUncommitedXml, newText);

                    setupInfo.CerebelloUncommitedXml = new SetupInfo.Version(1);
                }
                catch
                {
                }
            }

            if (force || !setupInfo.FirestarterUncommitedConfig.IsGreaterOrEqual(1))
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

            if (force || !setupInfo.TestsUncommitedConfig.IsGreaterOrEqual(1))
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

            if (force || !setupInfo.WindowsAzureTargets.IsGreaterOrEqual(1))
            {
                try
                {
                    var azureTargets = setupInfo.AzureTargets;

                    foreach (var azureTarget in azureTargets)
                    {
                        var dllsToLetIn = new[]
                            {
                                "System.EnterpriseServices.Wrapper.dll",
                            };

                        var neededAnds =
                            dllsToLetIn
                                .OrderBy(s => s)
                                .Select(s => string.Format("'%(WebFiles.Filename)%(WebFiles.Extension)' != '{0}'", s))
                                .ToArray();

                        var lines = File.ReadAllLines(azureTarget.FullName);
                        bool changed = false;
                        for (int itLine = 0; itLine < lines.Length; itLine++)
                        {
                            var curLine = lines[itLine];

                            if (curLine.Contains("<_AssembliesToValidate")
                                && curLine.Contains("Include=\"@(WebFiles);@(WorkerFiles)\""))
                            {
                                var match = Regex.Match(curLine, @"Condition\=""(?<CONDITION>(?:\\""|.)*?)""");
                                var oldCondition = match.Groups["CONDITION"].Value;

                                var oldAnds = oldCondition.Split(new[] { " AND ", " And ", " and " }, StringSplitOptions.None);

                                var itemsAnd = new List<string>();

                                foreach (var currentAnd in oldAnds)
                                    if (!itemsAnd.Contains(currentAnd))
                                        itemsAnd.Add(currentAnd.Trim());

                                var oldCount = itemsAnd.Count;

                                foreach (var neededAnd in neededAnds)
                                    if (!itemsAnd.Contains(neededAnd))
                                        itemsAnd.Add(neededAnd.Trim());

                                var newCount = itemsAnd.Count;

                                // if any needed AND item is missing, we must change the line
                                if (oldCount < newCount)
                                {
                                    var newLine = Regex.Replace(
                                        lines[itLine],
                                        @"Condition\=""(?<CONDITION>(?:\\""|.)*?)""",
                                        string.Format("Condition=\" {0} \"", string.Join(" And ", itemsAnd)));

                                    lines[itLine] = newLine;

                                    changed = true;
                                }
                            }
                        }

                        if (changed)
                            File.WriteAllLines(azureTarget.FullName, lines);

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
            var parser = new SimpleParser(m.Groups[1].Value)
                {
                    GlobalType = data.GetType(),
                    StaticTypes = new[] { typeof(Path) },
                };
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
