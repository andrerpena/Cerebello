using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Validation;

namespace CerebelloWebRole.Models
{
    public class CreateAccountViewModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Complete name of the person being registered in the software. Sample: "João Paulo da Cunha Santiago Neto".
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Nome Completo")]
        public String FullName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Data de Nascimento")]
        public DateTime? DateOfBirth { get; set; }

        // todo: This view-model should contain the BirthPlace property as well.

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeGender))]
        [Display(Name = "Sexo")]
        public short? Gender { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "E-Mail")]
        [DataType(DataType.EmailAddress)]
        [Remote("EmailIsAvailable", "Membership")]
        public String EMail { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Display(Name = "Confirmação da Senha")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "Confirmação da senha não bate")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// User name that may be use to login.
        /// The user can also use the e-mail to login.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Nome de usuário")]
        [UserNameAttribute]
        public string UserName { get; set; }

        /// <summary>
        /// Name of the practice being created along with the user.
        /// If this is null, then the current practice will be used.
        /// When no current medical practice exists, a name must be provided for the new practice.
        /// </summary>
        [Display(Name = "Nome do consultório")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string PracticeName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeTimeZone))]
        [Display(Name = "Fuso-horário do consultório")]
        public short? PracticeTimeZone { get; set; }

        [Display(Name = "Sou médico neste consultório")]
        public bool IsDoctor { get; set; }

        // Information of this user when he/she is a medic.
        // If IsDoctor is false, these properties have no meaning, so the validation can be discarded by the code.

        [Display(Name = "Número de registro no conselho")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicCRM { get; set; }

        [Display(Name = "Especialidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalSpecialtyId { get; set; }

        [Display(Name = "Especialidade")]
        public string MedicalSpecialtyName { get; set; }

        [Display(Name = "Conselho médico")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalEntityId { get; set; }

        [Display(Name = "Estado do conselho")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeEstadoBrasileiro))]
        public int? MedicalEntityJurisdiction { get; set; }
    }
}
