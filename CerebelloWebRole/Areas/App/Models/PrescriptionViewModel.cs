using System;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PrescriptionViewModel
    {
        [Display(Name = "Patient")]
        public int PatientId { get; set; }
        [Display(Name = "Patient")]
        public string PatientName { get; set; }
        [Display(Name = "Prescription")]
        public string Prescription { get; set; }
        [Display(Name = "Date")]
        public DateTime Date { get; set; }
        [Display(Name = "Quantity")]
        public string Quantity { get; set; }
    }
}