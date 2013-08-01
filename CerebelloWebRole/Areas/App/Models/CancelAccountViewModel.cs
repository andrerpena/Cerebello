using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class CancelAccountViewModel : ConfigAccountViewModel
    {
        /// <summary>
        /// Description of the reason for canceling the account.
        /// </summary>
        [Display(Name = "Canceling reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Must be true to effectively cancel the account.
        /// </summary>
        [Display(Name = "Confirm account canceling")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public bool Confirm { get; set; }

        /// <summary>
        /// Sends the patients data for each doctor by e-mail before canceling the account.
        /// </summary>
        [Display(Name = "Send data through e-mail")]
        public bool SendDataByEmail { get; set; }
    }
}
