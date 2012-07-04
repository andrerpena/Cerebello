using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DoctorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UrlIdentifier { get; set; }
        public string CRM { get; set; }
        public string MedicalSpecialty { get; set; }
        public string MedicalEntity { get; set; }
        public string ImageUrl { get; set; }

        public DateTime? NextAvailableTime { get; set; }
    }
}
