using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("ExaminationRequest", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("ExaminationRequest")]
    public class ExaminationRequestViewModel
    {
        /// <summary>
        /// Id of the examination request.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Id of the patient.
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// Id of the medical procedure.
        /// </summary>
        [Display(Name = "Procedimento")]
        public int? MedicalProcedureId { get; set; }

        /// <summary>
        /// Name of the medical procedure.
        /// </summary>
        [Display(Name = "Procedimento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalProcedureName { get; set; }

        /// <summary>
        /// Code of the medical procedure.
        /// </summary>
        [Display(Name = "Código CBHPM")]
        public string MedicalProcedureCode { get; set; }

        /// <summary>
        /// Notes for the examination request.
        /// </summary>
        [Display(Name = "Notas")]
        public string Notes { get; set; }

        [Display(Name = "Data da requisição")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public DateTime? RequestDate { get; set; }
    }
}
