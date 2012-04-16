using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AnamneseViewModel
    {
        public AnamneseViewModel()
        {
            this.AnamneseSymptoms = new List<AnamneseSymptomViewModel>();
        }

        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        public DateTime? Date { get; set; }

        [Display(Name="Texto")]
        public string Text { get; set; }

        [Display(Name="Sintomas")]
        public List<AnamneseSymptomViewModel> AnamneseSymptoms { get; set; }
    }
}