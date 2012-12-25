using System.Collections.Generic;
using System.Xml.Serialization;

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
    }
}