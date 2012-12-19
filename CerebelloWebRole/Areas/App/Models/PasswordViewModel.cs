using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PasswordViewModel
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Informe a sua senha atual.")]
        [Display(Name = "Senha atual")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Informe uma senha válida.")]
        [Display(Name = "Senha desejada")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Repita a senha.")]
        [Display(Name = "Repita a senha desejada")]
        public string RepeatPassword { get; set; }
    }
}
