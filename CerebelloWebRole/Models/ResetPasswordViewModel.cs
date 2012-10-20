using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Models
{
    public class ResetPasswordViewModel
    {
        public ResetPasswordViewModel()
        {
        }

        public ResetPasswordViewModel(string practiceIdentifier, string userNameOrEmail, string token)
        {
            this.PracticeIdentifier = practiceIdentifier;
            this.UserNameOrEmail = userNameOrEmail;
            this.Token = token;
        }

        public ResetPasswordViewModel(string practiceIdentifier, string userNameOrEmail)
        {
            this.PracticeIdentifier = practiceIdentifier;
            this.UserNameOrEmail = userNameOrEmail;
        }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Identificador do consultório")]
        public String PracticeIdentifier { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Nome de usuário ou e-mail")]
        public String UserNameOrEmail { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Token")]
        public String Token { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Nova senha")]
        [DataType(DataType.Password)]
        public String NewPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Confirmação da nova senha")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "Confirmação da nova senha não bate")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}