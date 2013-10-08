using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("PastMedicalHistory", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("PastMedicalHistory")]
    public class PastMedicalHistoryViewModel
    {
        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        [Display(Name = "Major events, hispitalizations, surgeries")]
        public string MajorEvents { get; set; }

        [Display(Name = "Allergies (free text)")]
        public string Allergies { get; set; }

        [Display(Name = "Ongoing medical problems")]
        public string OngoinMedicationProblems { get; set; }

        [Display(Name = "Family medical history")]
        public string FamilyMedicalHistory { get; set; }

        [Display(Name = "Preventive care")]
        public string PreventiveCare { get; set; }

        [Display(Name = "Social history")]
        public string SocialHistory { get; set; }

        [Display(Name = "Nutrition history")]
        public string NutritionHistory { get; set; }

        [Display(Name = "Development history")]
        public string DevelopmentHistory { get; set; }

        [Display(Name = "Record date")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}
