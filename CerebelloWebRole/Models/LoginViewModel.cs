using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Identificador do consultório")]
        public String PracticeIdentifier { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Nome de usuário ou e-mail")]
        public String UserNameOrEmail { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Display(Name = "Lembrar de mim neste computador")]
        public bool RememberMe { get; set; }
    }
}