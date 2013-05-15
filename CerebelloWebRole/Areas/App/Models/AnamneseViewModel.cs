using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Model.Metadata;

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

        [Display(Name = "Queixa principal (QP)")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string ChiefComplaint { get; set; }

        [Display(Name = "História da doença atual (HDA)")]
        public string HistoryOfThePresentIllness { get; set; }

        [Display(Name = "História médica/patológica pregressa (HMP ou HPP)")]
        public string PastMedicalHistory { get; set; }

        [Display(Name = "Revisão de sistemas")]
        public string ReviewOfSystems { get; set; }

        [Display(Name = "Histórico familiar (HF)")]
        public string FamilyDeseases { get; set; }

        [Display(Name = "História pessoal (fisiológica) e história social")]
        public string SocialHistory { get; set; }

        [Display(Name = "Medicações de uso regular")]
        public string RegularAndAcuteMedications { get; set; }

        [Display(Name = "Alergias")]
        public string Allergies { get; set; }

        [Display(Name = "Histórico Sexual")]
        public string SexualHistory { get; set; }

        [Display(Name = "Conclusão e fechamento")]
        public string Conclusion { get; set; }

        [Display(Name = "Sintomas")]
        public List<DiagnosticHypothesisViewModel> DiagnosticHypotheses { get; set; }

        [Display(Name = "Data de registro")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que a anamnese foi registrada")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}
