using System.Collections.Generic;
using CerebelloWebRole.Code.WindowsAzure;

namespace CerebelloWebRole.Models
{
    public class LogViewModel
    {
        public LogViewModel Clone()
        {
            return (LogViewModel)this.MemberwiseClone();
        }

        public string Message { get; set; }

        public List<WindowsAzureLogHelper.TraceLogsEntity> Logs { get; set; }

        public string FilterPath { get; set; }

        public string FilterSource { get; set; }

        public string FilterText { get; set; }

        public string FilterRoleInstance { get; set; }

        public string FilterLevel { get; set; }
    }
}
