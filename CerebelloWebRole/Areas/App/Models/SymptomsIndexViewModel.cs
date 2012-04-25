using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class SymptomsIndexViewModel
    {
        /// <summary>
        /// List of most common symptoms
        /// </summary>
        public List<SymptomViewModel> MostCommonSymptoms { get; set; }

        /// <summary>
        /// Count of the most common symptoms
        /// </summary>
        public int MostCommonSymptomsCount { get; set; }
    }
}