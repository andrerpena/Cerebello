using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("PhysicalExamination", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("PhysicalExamination")]
    public class PhysicalExaminationViewModel
    {
        [Display(Name = "Physical exam")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Notes { get; set; }

        public int? PatientId { get; set; }

        public int? Id { get; set; }

        [Display(Name = "Registry date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que o exame físico foi registrado")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}