using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PasswordViewModel
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please type in your current password")]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please type a valid password")]
        [Display(Name = "New password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "The new password must be confirmed")]
        [Display(Name = "Confirm new password")]
        public string RepeatPassword { get; set; }
    }
}
