using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// Phone view model
    /// </summary>
    public class PhoneViewModel
    {
        [Key]
        public int? Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Tipo")]
        [EnumDataType(typeof(TypePhone))]
        public int Type { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Nome completo")]
        public String Number { get; set; }
    }
}