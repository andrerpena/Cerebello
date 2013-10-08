using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AddressViewModel
    {
        [Display(Name = "Address Line 1")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public String AddressLine2 { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String City { get; set; }

        [Display(Name = "County")]
        public String County { get; set; }

        [Display(Name = "State")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String StateProvince { get; set; }

        [Display(Name = "ZIP")]
        public String ZipCode { get; set; }
    }
}