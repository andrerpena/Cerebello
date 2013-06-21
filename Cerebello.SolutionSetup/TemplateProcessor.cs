using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Cerebello.SolutionSetup.Parser;

namespace Cerebello.SolutionSetup
{
    class TemplateProcessor
    {
        public static string ProcessTemplate(string template, object data, bool htmlEncode)
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