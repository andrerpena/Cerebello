using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AddressViewModel
    {
        [Display(Name = "Street")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String Street { get; set; }

        [Display(Name = "Complement")]
        public String Complement { get; set; }

        [Display(Name = "Neighbourhood")]
        public String Neighborhood { get; set; }

        [Display(Name = "State")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String StateProvince { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String City { get; set; }

        [Display(Name = "ZIP")]
        public String CEP { get; set; }
    }
}