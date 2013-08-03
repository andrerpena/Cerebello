using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

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
        [Display(Name = "Exam / Proceeding")]
        [Tooltip("Exame ou procedimento relativo a este resultado. É possível cadastrar exames da tabela CBHPM ou texto livre. Ao começar a digitar, o sistema irá sugerir exames da tabela CBHPM. Caso um seja selecionado, este resultado será associada à tabela CBHPM")]
        public int? MedicalProcedureId { get; set; }

        /// <summary>
        /// Name of the medical procedure.
        /// </summary>
        [Display(Name = "Exam / Proceeding")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalProcedureName { get; set; }

        /// <summary>
        /// Code of the medical procedure.
        /// </summary>
        [Display(Name = "CBHPM code")]
        public string MedicalProcedureCode { get; set; }

        /// <summary>
        /// Text of the examination request.
        /// </summary>
        [Display(Name = "Result")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Text { get; set; }

        [Display(Name = "Exam / Proceeding date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Date the exam / proceeding has been done")]
        public DateTime? ExaminationDate { get; set; }

        [Display(Name = "Receiving date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Date the exam / proceeding has been registered")]
        public DateTime? ReceiveDate { get; set; }
    }
}
