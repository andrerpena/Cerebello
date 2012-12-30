using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Models
{
    public class VerifyPracticeAndEmailViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [RegularExpression(@"^(\d+)-([\da-fA-F]{32})$")]
        public string Token { get; set; }

        public string Practice { get; set; }

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
