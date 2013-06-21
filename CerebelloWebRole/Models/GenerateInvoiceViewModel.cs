using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Models
{
    public class GenerateInvoiceViewModel
    {
        public int? Step { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Identificador do consultório")]
        public String PracticeIdentifier { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Data de vencimento")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Valor")]
        public decimal? Amount { get; set; }

        // PayPal invoice form informations
        public PayPalInvoiceInfo Invoice { get; set; }

        public class PayPalInvoiceInfo
        {
            [Display(Name = "Endereço de e-mail do destinatário")]
            public string UserEmail { get; set; }

            [Display(Name = "Número da fatura")]
            public string Number { get; set; }

            [Display(Name = "Data da fatura")]
            public string IssuanceDate { get; set; }

            [Display(Name = "Termos de pagamento")]
            public string Terms { get; set; }

            [Display(Name = "Data de vencimento")]
            public string DuaDate { get; set; }

            [Display(Name = "Moeda")]
            public string Currency { get; set; }

            public List<PayPalInvoiceItem> Items { get; set; }
        }

        public class PayPalInvoiceItem
        {
            [Display(Name = "Nome/ID do Prod.")]
            public string NameId { get; set; }

            [Display(Name = "Data")]
            public string Date { get; set; }

            [Display(Name = "Quantidade")]
            public int Quantity { get; set; }

            [Display(Name = "Preço unitário")]
            public string UnitPrice { get; set; }

            [Display(Name = "Descrição")]
            public string Description { get; set; }
        }

        public List<Code.BillingInfo> MissingInvoices { get; set; }
    }
}
