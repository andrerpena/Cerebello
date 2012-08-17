using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ExaminationRequestViewModel
    {
        public ExaminationRequestViewModel()
        {
        }

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
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int MedicalProcedureId { get; set; }

        /// <summary>
        /// Text of the medical procedure.
        /// </summary>
        [Display(Name = "Procedimento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalProcedureText { get; set; }

        /// <summary>
        /// Code of the medical procedure.
        /// </summary>
        [Display(Name = "Código do procedimento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalProcedureCode { get; set; }

        /// <summary>
        /// Notes for the examination request.
        /// </summary>
        [Display(Name = "Notas")]
        public string Notes { get; set; }
    }
}