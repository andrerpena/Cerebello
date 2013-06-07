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
        [Display(Name = "Exame / Procedimento")]
        [Tooltip("Exame ou procedimento relativo a este resultado. É possível cadastrar exames da tabela CBHPM ou texto livre. Ao começar a digitar, o sistema irá sugerir exames da tabela CBHPM. Caso um seja selecionado, este resultado será associada à tabela CBHPM")]
        public int? MedicalProcedureId { get; set; }

        /// <summary>
        /// Name of the medical procedure.
        /// </summary>
        [Display(Name = "Exame / Procedimento")]
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

        [Display(Name = "Data do exame")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que o exame foi realizado")]
        public DateTime? ExaminationDate { get; set; }

        [Display(Name = "Data de recebimento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que o resultado foi cadastrado")]
        public DateTime? ReceiveDate { get; set; }
    }
}
