using System;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PrescriptionViewModel
    {
        [Display(Name = "Paciente")]
        public int PatientId { get; set; }
        [Display(Name = "Paciente")]
        public string PatientName { get; set; }
        [Display(Name = "Prescrição")]
        public string Prescription { get; set; }
        [Display(Name = "Data")]
        public DateTime Date { get; set; }
        [Display(Name = "Quantidade")]
        public string Quantity { get; set; }
    }
}