using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Mvc;

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
        [Display(Name = "Nome completo")]
        public String FullName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Sexo")]
        [EnumDataType(typeof(TypeGender))]
        [XmlIgnore]
        public int Gender { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Data de Nascimento")]
        [DateOfBirth]
        public DateTime DateOfBirth { get; set; }

        [EnumDataType(typeof(TypeMaritalStatus))]
        [Display(Name = "Estado Civil")]
        [XmlIgnore]
        public short? MaritalStatus { get; set; }

        [Display(Name = "Naturalidade")]
        public String BirthPlace { get; set; }

        [Display(Name = "CPF")]
        public String Cpf { get; set; }

        [Display(Name = "Proprietário do CPF")]
        [EnumDataType(typeof(TypeCpfOwner))]
        [XmlIgnore]
        public short? CpfOwner { get; set; }

        [Display(Name = "Profissão")]
        public String Profissao { get; set; }

        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        [Code.Mvc.EmailAddress]
        public String Email { get; set; }

        [Display(Name = "Telefone fixo")]
        [UIHint("Phone")]
        public String PhoneLand { get; set; }

        [Display(Name = "Telefone celular")]
        [UIHint("Phone")]
        public String PhoneCell { get; set; }

        [Display(Name = "Endereço")]
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