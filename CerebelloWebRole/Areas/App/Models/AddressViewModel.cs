using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AddressViewModel
    {
        [Display(Name = "Logradouro")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String Street { get; set; }

        [Display(Name = "Complemento")]
        public String Complement { get; set; }

        [Display(Name = "Bairro")]
        public String Neighborhood { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String StateProvince { get; set; }

        [Display(Name = "Cidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String City { get; set; }

        [Display(Name = "CEP")]
        public String CEP { get; set; }
    }
}