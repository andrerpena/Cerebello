using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ScheduleEventViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public bool allDay { get; set; }
        public string className { get; set; }
    }
}