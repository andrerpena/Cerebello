using System;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DoctorViewModel
    {
        public int Id { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public string UrlIdentifier { get; set; }

        [Display(Name = "CRM")]
        public string CRM { get; set; }

        [Display(Name = "Specialty")]
        public string MedicalSpecialty { get; set; }

        [Display(Name = "Medical entity")]
        public string MedicalEntity { get; set; }
        public string ImageUrl { get; set; }

        public bool IsScheduleConfigured { get; set; }
        public DateTime? NextAvailableTime { get; set; }
    }
}
