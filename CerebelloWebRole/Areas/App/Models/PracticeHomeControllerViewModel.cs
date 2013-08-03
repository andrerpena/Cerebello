using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PracticeHomeControllerViewModel
    {
        public PracticeHomeControllerViewModel()
        {
            this.Address = new AddressViewModel();
        }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeTimeZone))]
        [Display(Name = "Practice time-zone")]
        public short PracticeTimeZone { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [RegularExpression(@"^\s*[\w\p{P}\d]+(\s+[\w\p{P}\d]+)*\s*$",
            ErrorMessage = "Nome de consultório precisa conter letras, números, ou alguma pontuação.")]
        [Display(Name = "Practice name")]
        public string PracticeName { get; set; }

        [Display(Name = "Land line")]
        [UIHint("Phone")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string PhoneMain { get; set; }

        [Display(Name = "Land line (alternative)")]
        [UIHint("Phone")]
        public string PhoneAlt { get; set; }

        [Display(Name = "Pabx")]
        [UIHint("Phone")]
        public string Pabx { get; set; }

        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        [Code.EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Site")]
        public string SiteUrl { get; set; }

        [Display(Name = "Address")]
        public AddressViewModel Address { get; set; }

        public List<DoctorViewModel> Doctors { get; set; }

        public List<UserViewModel> Users { get; set; }
    }
}
