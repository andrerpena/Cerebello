using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// Modelo de receita médica (cada receita emitida pertence a um desses modelos)
    /// </summary>
    public class ModelMedicalCertificateViewModel
    {
        public int? Id { get; set; }

        /// <summary>
        /// Nome do modelo de receita médica
        /// </summary>
        [Display(Name = "Nome")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceName="MaxLengthValidationMessage")]
        public string Name { get; set; }

        /// <summary>
        /// Texto da receita
        /// </summary>
        [Display(Name = "Texto")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Text { get; set; }
    }
}