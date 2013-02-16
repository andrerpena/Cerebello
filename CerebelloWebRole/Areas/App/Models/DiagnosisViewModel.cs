using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Diagnosis", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Diagnosis")]
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
        [Display(Name = "CID-10")]
        public string Cid10Code { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "Diagnóstico")]
        public string Cid10Name { get; set; }
    }
}