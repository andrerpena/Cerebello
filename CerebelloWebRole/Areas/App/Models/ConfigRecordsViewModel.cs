using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ConfigRecordsViewModel
    {
        public ConfigRecordsViewModel()
        {
            this.Records = new List<ConfigRecordViewModel>();
        }

        public List<ConfigRecordViewModel> Records { get; set; }
    }
}