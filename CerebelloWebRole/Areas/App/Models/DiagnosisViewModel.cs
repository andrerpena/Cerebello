using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DiagnosisViewModel
    {
        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        [Display(Name = "Notas")]
        public string Text { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "Diagnóstico")]
        public string Cid10Code { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "Diagnóstico")]
        public string Cid10Name { get; set; }
    }
}