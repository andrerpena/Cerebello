using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicalCertificateFieldViewModel
    {
        public int? Id { get; set; }

        /// <summary>
        /// Field name. This is going to be a hidden field. It's just a REFERENCE 
        /// </summary>
        [Display(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Field value
        /// </summary>
        [Display(Name = "Value")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(50, ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string Value { get; set; }
    }
}