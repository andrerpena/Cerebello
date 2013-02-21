using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("ExaminationResult", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("ExaminationResult")]
    public class ExaminationResultViewModel
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
        /// Text of the examination request.
        /// </summary>
        [Display(Name = "Resultado")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Text { get; set; }
    }
}
