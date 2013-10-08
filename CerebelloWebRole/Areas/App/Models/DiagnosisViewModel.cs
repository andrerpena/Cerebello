using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Diagnosis", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Diagnosis")]
    public class DiagnosisViewModel
    {
        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "DX")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("É possível usar tando doenças/condições listadas no CID-10 quanto texto livre. Ao começar a digitar, o sistema irá sugerir itens da lista do CID-10. Caso um desses itens seja selecionado, o código CID-10 será associado a este diagnóstico.")]
        public string Code { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "DX")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Name { get; set; }

        [Display(Name = "Start time")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End time")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Acute")]
        public bool Acute { get; set; }

        [Display(Name = "Is terminal")]
        public bool IsTerminal { get; set; }

        [Display(Name = "Comments")]
        public string Comments { get; set; }

        [Display(Name = "Record date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("This is mainly used for registering past records")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}