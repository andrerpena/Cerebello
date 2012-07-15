using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// Import from anvisa view model
    /// </summary>
    public class AnvisaImportViewModel
    {
        public int AnvisaId { get; set; }
        public string AnvisaText { get; set; }
    }
}