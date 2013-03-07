using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// The change contract view model.
    /// </summary>
    public class ChangeContractViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether user accepted the contract or not.
        /// </summary>
        [Display(Name = "Li e aceito os termos do contrato")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public bool AcceptedByUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the payment model selected by the user.
        /// The payment model indicates the frequency and values of payments.
        /// </summary>
        [Display(Name = "Forma de pagamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string PaymentModelName { get; set; }

        /// <summary>
        /// Gets or sets the promotional code entered by the user, when the selected payment model requires this.
        /// </summary>
        [Display(Name = "Código promocional")]
        public string PromotionalCode { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        [Display(Name = "Meio de pagamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the payment invoice due day-of-month.
        /// The user may choose the best day for him/her.
        /// </summary>
        [Display(Name = "Dia de vencimento da fatura")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? InvoceDueDayOfMonth { get; set; }

        /// <summary>
        /// Gets or sets the contract url id.
        /// </summary>
        public string ContractUrlId { get; set; }

        /// <summary>
        /// Gets or sets the desired size of the practice.
        /// </summary>
        [Display(Name = "Quantidade de médicos")]
        public int DoctorCount { get; set; }

        /// <summary>
        /// Gets or sets the currently registered doctors.
        /// </summary>
        public int CurrentDoctorsCount { get; set; }

        /// <summary>
        /// Gets or sets the final value calculated by the java-script code, and sent to the server.
        /// This is needed because this is the value that the person sees at the client.
        /// This value must be validated at the server to prevent fraud.
        /// </summary>
        public decimal FinalValue { get; set; }

        /// <summary>
        /// Gets or sets the whole HTML of the user agreement, that was seen by the user.
        /// </summary>
        public string WholeUserAgreement { get; set; }
    }
}
