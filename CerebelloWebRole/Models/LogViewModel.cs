using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CerebelloWebRole.Models
{
    public class LogViewModel
    {
        public LogViewModel Clone()
        {
            var result = (LogViewModel)this.MemberwiseClone();
            if (result.FilterSpecial != null)
            {
                result.FilterSpecial = new HashSet<string>(result.FilterSpecial).ToList();
            }

            result.Logs = null;

            return result;
        }

        private int page;
        public int? Page
        {
            get { return this.page <= 1 ? (int?)null : this.page; }
            set { this.page = Math.Max(value ?? 1, 1); }
        }

        public string Message { get; set; }

        public List<TraceLogItem> Logs { get; set; }

        public string FilterPath { get; set; }

        public string FilterSource { get; set; }

        public string FilterRoleInstance { get; set; }

        public int? FilterLevel { get; set; }

        public List<string> FilterSpecial { get; set; }

        public string FilterText { get; set; }

        public class TraceLogItem
        {
            public TraceLogItem(string message)
            {
                var match = Regex.Match(
                    message,
                    @"^(?:(?<NODES>.*?)\:)?(?<TEXT>.*?)(?:;\sTraceSource\s\'(?<SOURCE>.*?)\'\sevent)?$",
                    RegexOptions.Singleline);

                this.Message = message;
                var nodes = match.Groups["NODES"].Value;
                nodes = string.Join(".", nodes.Split('.').Select(s => s.Trim()));
                this.Path = nodes;
                this.Text = match.Groups["TEXT"].Value;
                this.Source = match.Groups["SOURCE"].Value;

                var match2 = Regex.Matches(this.Text, @"\[(.*?)\]");
                this.SpecialStrings = new HashSet<string>(match2.OfType<Match>().Select(m => m.Groups[1].ToString()));
            }

            public DateTime Timestamp { get; set; }
            public int Level { get; set; }
            public string RoleInstance { get; set; }
            public string Message { get; set; }
            public string Source { get; set; }
            public string Path { get; set; }
            public string Text { get; set; }
            public string Role { get; set; }
            public HashSet<string> SpecialStrings { get; set; }
        }

        public bool HasAnyFilter()
        {
            return this.HasLevelFilter()
                || this.HasPathFilter()
                || this.HasRoleInstanceFilter()
                || this.HasSourceFilter()
                || this.HasSpecialFilter()
                || this.HasTextFilter();
        }

        public bool HasLevelFilter() { return this.FilterLevel != null; }
        public bool HasPathFilter() { return !string.IsNullOrWhiteSpace(this.FilterPath); }
        public bool HasRoleInstanceFilter() { return !string.IsNullOrWhiteSpace(this.FilterRoleInstance); }
        public bool HasSourceFilter() { return !string.IsNullOrWhiteSpace(this.FilterSource); }
        public bool HasSpecialFilter() { return this.FilterSpecial != null && this.FilterSpecial.Any(s => !string.IsNullOrWhiteSpace(s)); }
        public bool HasTextFilter() { return !string.IsNullOrWhiteSpace(this.FilterText); }
    }
}
