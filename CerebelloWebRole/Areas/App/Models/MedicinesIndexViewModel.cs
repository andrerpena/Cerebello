using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicinesIndexViewModel
    {
        /// <summary>
        /// List of last registered medicines
        /// </summary>
        public List<MedicineViewModel> LastRegisteredMedicines { get; set; }

        /// <summary>
        /// Total number of medicines
        /// </summary>
        public int TotalMedicinesCount { get; set; }
    }
}