using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ChangeContractViewModel
    {
        /// <summary>
        /// Indicates whether user accepted the contract or not.
        /// </summary>
        [Display(Name = "Aceito os termos do contrato")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public bool AcceptedByUser { get; set; }

        /// <summary>
        /// Indicates the payment model selected by the user.
        /// The payment model indicates the frequency and values of payments.
        /// </summary>
        [Display(Name = "Forma de pagamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string PaymentModelName { get; set; }

        /// <summary>
        /// Promotional code entered by the user, when the selected payment model requires this.
        /// </summary>
        [Display(Name = "Código promocional")]
        public string PromotionalCode { get; set; }

        [Display(Name = "Meio de pagamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Indicates the payment invoice due day-of-month.
        /// The user may choose the best day for him/her.
        /// </summary>
        [Display(Name = "Dia de vencimento da fatura")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? InvoceDueDayOfMonth { get; set; }

        public string ContractUrlId { get; set; }

        [Display(Name = "Quantidade de médicos")]
        public int DoctorCount { get; set; }
    }
}
