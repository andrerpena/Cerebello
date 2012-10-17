using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cerebello.Firestarter
{
    public static class SqlHelper
    {
        /// <summary>
        /// Splits a SQL script containing GO statements.
        /// </summary>
        /// <param name="script"></param>
        public static string[] SplitScript(string script)
        {
            var scripts = Regex.Split(script, @"(?<=(?:[\r\n]|^)+\s*)GO(?=\s*(?:[\r\n]|$))", RegexOptions.IgnoreCase);
            return scripts;
        }

        /// <summary>
        /// Changes the DB creation script, to create all text columns with a specific collation.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="collation"></param>
        /// <returns></returns>
        public static string SetScriptColumnsCollation(string script, string collation)
        {
            var result = Regex.Replace(
                script,
                @"(\[.*?\]\s+\[(?:varchar|char|nchar|nvarchar|text|ntext)\](?:\(\d+\))?)(?:\s+COLLATE\s+.*?)?\s+(NOT NULL|NULL)",
                m => string.Format("{0} COLLATE {1} {2}", m.Groups[1].Value, collation, m.Groups[2].Value),
                RegexOptions.IgnoreCase);

            return result;
        }
    }
}
