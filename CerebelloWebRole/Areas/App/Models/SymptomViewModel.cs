using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Symptom", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Symptom")]
    public class SymptomViewModel
    {
        [Display(Name = "Sintoma")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Text { get; set; }

        /// <summary>
        /// Categoria
        /// </summary>
        [Display(Name = "Cid10")]
        public string Cid10Code { get; set; }
    }
}
