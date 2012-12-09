using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DoctorViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nome completo")]
        public string Name { get; set; }
        public string UrlIdentifier { get; set; }

        [Display(Name = "CRM")]
        public string CRM { get; set; }

        [Display(Name = "Especialidade")]
        public string MedicalSpecialty { get; set; }

        [Display(Name = "Conselho")]
        public string MedicalEntity { get; set; }
        public string ImageUrl { get; set; }

        public bool IsScheduleConfigured { get; set; }
        public DateTime? NextAvailableTime { get; set; }
    }
}
