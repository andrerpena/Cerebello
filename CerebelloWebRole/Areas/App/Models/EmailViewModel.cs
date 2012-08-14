using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class EmailViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Endereço")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.EmailAddress)]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String Address { get; set; }
    }
}