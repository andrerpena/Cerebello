using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;
using JetBrains.Annotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PersonViewModel
    {
        public PersonViewModel()
        {
            this.Address = new AddressViewModel();
        }

        /// <summary>
        /// Id of the object in the database.
        /// Note: this Id may not be a Person id, but a related entity id... e.g.: a Patient id, or a Doctor id.
        /// </summary>
        [Key]
        public int? Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "First name")]
        [CanBeNull]
        public String FirstName { get; set; }

        [Display(Name = "Middle name")]
        [CanBeNull]
        public String MiddleName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Last name")]
        [CanBeNull]
        public String LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Sex")]
        [EnumDataType(typeof(TypeGender))]
        [XmlIgnore]
        public int Gender { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Date of birth")]
        [DateOfBirth]
        // TODO: MVC 4 use HTML 5 <input type="date" for DateTime... this is a problem for IE9 and FireFox />
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Date of death")]
        // TODO: MVC 4 use HTML 5 <input type="date" for DateTime... this is a problem for IE9 and FireFox />
        public DateTime? DateOfDeath { get; set; }
        
        [Display(Name = "Social Security Number")]
        public String SSN { get; set; }

        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)] // TODO: This will make MVC 4 use HTML 5 <input type="email" />
        [Code.EmailAddress]
        [Tooltip("Email is used for automatic appointment reminders and other notifications")]
        public String Email { get; set; }

        [Display(Name = "Home phone")]
        [UIHint("Phone")]
        public String PhoneHome { get; set; }

        [Display(Name = "Mobile phone")]
        [UIHint("Phone")]
        public String PhoneMobile { get; set; }

        [Display(Name = "Work phone")]
        [UIHint("Phone")]
        public String PhoneWork { get; set; }

        [Display(Name = "Work phone ext.")]
        public String PhoneWorkExt { get; set; }

        [Display(Name = "Address")]
        public AddressViewModel Address { get; set; }
    }

    public class CEPInfo
    {
        public String StateProvince { get; set; }
        public String City { get; set; }
        public String Neighborhood { get; set; }
        public String Street { get; set; }
    }
}