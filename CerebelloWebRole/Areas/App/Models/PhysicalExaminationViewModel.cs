using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("PhysicalExamination", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("PhysicalExamination")]
    public class PhysicalExaminationViewModel
    {
        [Display(Name = "Exame físico")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Notes { get; set; }

        public int? PatientId { get; set; }

        public int? Id { get; set; }

        [Display(Name = "Data de registro")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}