using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cerebello.SolutionSetup
{
    class SetupSteps
    {
        public static void WindowsAzureTargets(SetupInfo setupInfo)
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
                        .Select(s => String.Format("'%(WebFiles.Filename)%(WebFiles.Extension)' != '{0}'", s))
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
                                String.Format("Condition=\" {0} \"", String.Join(" And ", itemsAnd)));

                            lines[itLine] = newLine;

                            changed = true;
                        }
                    }
                }

                if (changed)
                    File.WriteAllLines(azureTarget.FullName, lines);
            }
        }

        public static void TestUncommitedConfig(SetupInfo setupInfo)
        {
            var testUncommitedConfigTemplate = Path.Combine(
                setupInfo.Props.SolutionPath,
                @"CerebelloWebRole.Tests\Uncommited.template.config");

            var testUncommitedConfig = Path.Combine(
                setupInfo.Props.SolutionPath,
                @"CerebelloWebRole.Tests\Uncommited.config");

            var newText = TemplateProcessor.ProcessTemplate(File.ReadAllText(testUncommitedConfigTemplate), setupInfo.Props, true);
            File.WriteAllText(testUncommitedConfig, newText);
        }

        public static void FirestarterUncommitedConfig(SetupInfo setupInfo)
        {
            var fsUncommitedConfigTemplate = Path.Combine(
                setupInfo.Props.SolutionPath,
                @"Cerebello.Firestarter\Uncommited.template.config");

            var fsUncommitedConfig = Path.Combine(
                setupInfo.Props.SolutionPath,
                @"Cerebello.Firestarter\Uncommited.config");

            var newText = TemplateProcessor.ProcessTemplate(File.ReadAllText(fsUncommitedConfigTemplate), setupInfo.Props, true);
            File.WriteAllText(fsUncommitedConfig, newText);
        }

        public static void UncommitedXml(SetupInfo setupInfo)
        {
            var fsUncommitedXmlTemplate = Path.Combine(
                setupInfo.Props.SolutionPath,
                @"CerebelloWebRole\Uncommited.template.xml");

            var fsUncommitedXml = Path.Combine(
                setupInfo.Props.SolutionPath,
                @"CerebelloWebRole\Uncommited.xml");

            var newText = TemplateProcessor.ProcessTemplate(File.ReadAllText(fsUncommitedXmlTemplate), setupInfo.Props, true);
            File.WriteAllText(fsUncommitedXml, newText);
        }
    }
}