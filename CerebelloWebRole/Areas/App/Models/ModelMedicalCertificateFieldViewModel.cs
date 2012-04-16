using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// Um campo em um modelo de receita médica
    /// </summary>
    public class ModelMedicalCertificateFieldViewModel
    {
        public int? Id { get; set; }

        /// <summary>
        /// Nome do campo
        /// </summary>
        [Display(Name = "Nome")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string Name { get; set; }
    }
}