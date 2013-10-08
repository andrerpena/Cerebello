using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PatientBasicInformationViewModel
    {
        public int Id { get; set; }

        // Person information
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)] // TODO: This will make MVC 4 use HTML 5 <input type="email" />
        [Code.EmailAddress]
        [Tooltip("Email is used for automatic appointment reminders and other notifications")]
        public String PersonEmail { get; set; }

        [Display(Name = "Home phone")]
        [UIHint("Phone")]
        public String PersonPhoneHome { get; set; }

        [Display(Name = "Mobile phone")]
        [UIHint("Phone")]
        public String PersonPhoneMobile { get; set; }

        [Display(Name = "Work phone")]
        [UIHint("Phone")]
        public String PersonPhoneWork { get; set; }

        [Display(Name = "Work phone ext.")]
        public String PersonPhoneWorkExt { get; set; }

        // Address information
        [Display(Name = "Address Line 1")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String PersonAddressAddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public String PersonAddressAddressLine2 { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String PersonAddressCity { get; set; }

        [Display(Name = "County")]
        public String PersonAddressCounty { get; set; }

        [Display(Name = "State")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String PersonAddressStateProvince { get; set; }

        [Display(Name = "ZIP")]
        public String PersonAddressZipCode { get; set; }
    }
}