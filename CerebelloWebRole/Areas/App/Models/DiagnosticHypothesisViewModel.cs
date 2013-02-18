using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Symptom", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Symptom")]
    public class DiagnosticHypothesisViewModel
    {
        public int? Id { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "CID-10")]
        public string Cid10Code { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "Hipótese de diagnóstico")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Cid10Name { get; set; }

        [Display(Name = "Notas")]
        public string Text { get; set; }
    }
}
