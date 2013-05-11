using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Model.Metadata;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("DiagnosticHypothesis", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("DiagnosticHypothesis")]
    public class DiagnosticHypothesisViewModel
    {
        public int? Id { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "CID-10")]
        public string Cid10Code { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "Hipótese diagnóstica")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("É possível usar tando doenças/condições listadas no CID-10 quanto texto livre. Ao começar a digitar, o sistema irá sugerir itens da lista do CID-10. Caso um desses itens seja selecionado, o código CID-10 será associado a esta hipótese")]
        public string Cid10Name { get; set; }

        [Display(Name = "Notas")]
        public string Text { get; set; }

        [Required]
        public int? PatientId { get; set; }

        [Display(Name = "Data de registro")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que a hipótese diagnóstica foi registrada")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}
