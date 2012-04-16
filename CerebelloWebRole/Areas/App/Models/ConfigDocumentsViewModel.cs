using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ConfigDocumentsViewModel
    {
        [Display(Name = "Linha 1")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String Header1 { get; set; }

        [Display(Name = "Linha 2")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String Header2 { get; set; }

        [Display(Name = "Linha 1")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String FooterLeft1 { get; set; }

        [Display(Name = "Linha 2")]
        public String FooterLeft2 { get; set; }

        [Display(Name = "Linha 1")]
        public String FooterRight1 { get; set; }

        [Display(Name = "Linha 2")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String FooterRight2 { get; set; }
    }
}