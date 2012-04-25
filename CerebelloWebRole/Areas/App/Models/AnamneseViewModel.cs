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
            this.Diagnoses = new List<DiagnosisViewModel>();
        }

        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        public DateTime? Date { get; set; }

        [Display(Name="Texto")]
        public string Text { get; set; }

        [Display(Name="Diagnósticos")]
        public List<DiagnosisViewModel> Diagnoses { get; set; }
    }
}