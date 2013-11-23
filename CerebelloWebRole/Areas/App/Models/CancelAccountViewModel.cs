using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class CancelAccountViewModel : ConfigAccountViewModel
    {
        /// <summary>
        /// Description of the reason for canceling the account.
        /// </summary>
        [Display(Name = "Razões do cancelamento")]
        public string Reason { get; set; }

        /// <summary>
        /// Must be true to effectively cancel the account.
        /// </summary>
        [Display(Name = "Confirmar cancelamento da conta")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public bool Confirm { get; set; }

        /// <summary>
        /// Sends the patients data for each doctor by e-mail before canceling the account.
        /// </summary>
        [Display(Name = "Enviar dados por e-mail")]
        public bool SendDataByEmail { get; set; }
    }
}
