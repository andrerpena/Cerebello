using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
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
    }
}