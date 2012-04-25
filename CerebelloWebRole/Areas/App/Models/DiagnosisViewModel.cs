using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DiagnosisViewModel
    {
        [Display(Name = "Sintoma")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Text { get; set; }

        /// <summary>
        /// Categoria
        /// </summary>
        [Display(Name = "Cid10")]
        public string Cid10Code { get; set; }
    }
}