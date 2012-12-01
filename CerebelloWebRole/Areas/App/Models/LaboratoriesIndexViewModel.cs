using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class LaboratoriesIndexViewModel
    {
        /// <summary>
        /// List of last registered laboratories
        /// </summary>
        public List<MedicineLaboratoryViewModel> LastRegisteredLaboratories { get; set; }

        /// <summary>
        /// Total laboratories registered count
        /// </summary>
        public int TotalLaboratoriesCount { get; set; }
    }
}