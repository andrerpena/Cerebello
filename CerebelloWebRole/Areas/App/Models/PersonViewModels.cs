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
        [Display(Name = "Full name")]
        [CanBeNull]
        public String FullName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Gender")]
        [EnumDataType(typeof(TypeGender))]
        [XmlIgnore]
        public int Gender { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Date of birth")]
        [DateOfBirth]
        // TODO: MVC 4 use HTML 5 <input type="date" for DateTime... this is a problem for IE9 and FireFox />
        public DateTime DateOfBirth { get; set; }

        [EnumDataType(typeof(TypeMaritalStatus))]
        [Display(Name = "Marital status")]
        [XmlIgnore]
        public short? MaritalStatus { get; set; }

        [Display(Name = "Naturality")]
        public String BirthPlace { get; set; }

        [Display(Name = "Social Security Number")]
        public String Cpf { get; set; }

        [Display(Name = "Social Security Number owner")]
        [EnumDataType(typeof(TypeCpfOwner))]
        [XmlIgnore]
        public short? CpfOwner { get; set; }

        [Display(Name = "Profession")]
        public String Profession { get; set; }

        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)] // TODO: This will make MVC 4 use HTML 5 <input type="email" />
        [Code.EmailAddress]
        public String Email { get; set; }

        [Display(Name = "Land line")]
        [UIHint("Phone")]
        public String PhoneLand { get; set; }

        [Display(Name = "Cell phone")]
        [UIHint("Phone")]
        public String PhoneCell { get; set; }

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