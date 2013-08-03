using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Receipt", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Receipt")]
    public class ReceiptViewModel
    {
        public ReceiptViewModel()
        {
            this.ReceiptMedicines = new List<ReceiptMedicineViewModel>();
        }

        public int? Id { get; set; }
        public int? PatientId { get; set; }
        // Propriedade não está sendo usada?
        //public DateTime? Date { get; set; }

        public List<ReceiptMedicineViewModel> ReceiptMedicines { get; set; }

        [Display(Name = "Date of issue")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Date this prescription has been issue")]
        public DateTime? IssuanceDate { get; set; }
    }
}
