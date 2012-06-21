using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.Models;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PersonViewModel
    {
        public PersonViewModel()
        {
            this.Addresses = new List<AddressViewModel>();
            this.Emails = new List<EmailViewModel>();
        }

        [Key]
        public int? Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Nome completo")]
        public String FullName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Sexo")]
        [EnumDataType(typeof(TypeGender))]
        public int Gender { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Data de Nascimento")]
        [DateOfBirth]
        public DateTime DateOfBirth { get; set; }

        [EnumDataType(typeof(TypeMaritalStatus))]
        [Display(Name = "Estado Civil")]
        public short? MaritalStatus { get; set; }

        [Display(Name = "Naturalidade")]
        public String BirthPlace { get; set; }

        [Display(Name = "CPF")]
        public String CPF { get; set; }

        [Display(Name = "Proprietário do CPF")]
        [EnumDataType(typeof(TypeCPFOwner))]
        public short? CPFOwner { get; set; }

        [Display(Name = "Profissão")]
        public String Profissao { get; set; }

        [Display(Name = "Endereços")]
        public List<AddressViewModel> Addresses { get; set; }

        [Display(Name = "E-mails")]
        public List<EmailViewModel> Emails { get; set; }
    }

    public class CEPInfo
    {
        public String StateProvince { get; set; }
        public String City { get; set; }
        public String Neighborhood { get; set; }
        public String Street { get; set; }
    }
}