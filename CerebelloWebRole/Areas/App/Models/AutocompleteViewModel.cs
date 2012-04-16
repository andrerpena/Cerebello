using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AutocompleteViewModel
    {
        public int id { get; set; }
        public string url { get; set; }
        public string value { get; set; }
        public string description { get; set; }
    }
}