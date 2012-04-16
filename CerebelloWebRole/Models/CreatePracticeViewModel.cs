using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Models
{
    public class CreatePracticeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String Name { get; set; }
    }
}