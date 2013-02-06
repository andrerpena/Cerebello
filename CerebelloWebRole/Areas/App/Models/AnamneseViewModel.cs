using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Anamnese", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Anamnese")]
    public class AnamneseViewModel
    {
        public AnamneseViewModel()
        {
            this.Symptoms = new List<SymptomViewModel>();
        }

        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        [Display(Name = "Queixa principal (QP)")]
        public string ChiefComplaint { get; set; }

        [Display(Name = "História da doença atual (HDA)")]
        public string HistoryOfThePresentIllness { get; set; }

        [Display(Name = "História médica pregressa ou História patológica pregressa (HMP ou HPP)")]
        public string PastMedicalHistory { get; set; }

        [Display(Name = "Revisão de sistemas")]
        public string ReviewOfSystems { get; set; }

        [Display(Name = "Histórico familiar (HF)")]
        public string FamilyDeseases { get; set; }

        [Display(Name = "História pessoal (fisiológica) e história social")]
        public string SocialDeseases { get; set; }

        [Display(Name = "Medicações de uso regular")]
        public string RegularAndAcuteMedications { get; set; }

        [Display(Name = "Alergias")]
        public string Allergies { get; set; }

        [Display(Name = "Histórico Sexual")]
        public string SexualHistory { get; set; }

        [Display(Name = "Conclusão e fechamento")]
        public string Conclusion { get; set; }

        [Display(Name = "Sintomas")]
        public List<SymptomViewModel> Symptoms { get; set; }
    }
}
