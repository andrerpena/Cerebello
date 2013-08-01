using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Anamnese", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Anamnese")]
    public class AnamneseViewModel
    {
        public AnamneseViewModel()
        {
            this.DiagnosticHypotheses = new List<DiagnosticHypothesisViewModel>();
        }

        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        [Display(Name = "Chief complaint")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string ChiefComplaint { get; set; }

        [Display(Name = "History of the present illness")]
        public string HistoryOfThePresentIllness { get; set; }

        [Display(Name = "Past medical history")]
        public string PastMedicalHistory { get; set; }

        [Display(Name = "Review of systems")]
        public string ReviewOfSystems { get; set; }

        [Display(Name = "Family deseases")]
        public string FamilyDeseases { get; set; }

        [Display(Name = "Social history")]
        public string SocialHistory { get; set; }

        [Display(Name = "Regular and acute medications")]
        public string RegularAndAcuteMedications { get; set; }

        [Display(Name = "Allergies")]
        public string Allergies { get; set; }

        [Display(Name = "Sexual histry")]
        public string SexualHistory { get; set; }

        [Display(Name = "Conclusion")]
        public string Conclusion { get; set; }

        [Display(Name = "Symptoms")]
        public List<DiagnosticHypothesisViewModel> DiagnosticHypotheses { get; set; }

        [Display(Name = "Record date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("This is mainly used for registering past records")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}
