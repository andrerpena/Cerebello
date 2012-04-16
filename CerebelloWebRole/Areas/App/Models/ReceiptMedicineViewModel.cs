using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ReceiptMedicineViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Medicamento")]
        public int MedicineId { get; set; }

        [Display(Name = "Medicamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicineText { get; set; }

        [Display(Name="Quantidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Quantity { get; set; }

        [Display(Name = "Precrição")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Prescription { get; set; }

        [Display(Name = "Observações")]
        public string Observations { get; set; }
    }
}