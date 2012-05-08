using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class GlobalSearchViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}