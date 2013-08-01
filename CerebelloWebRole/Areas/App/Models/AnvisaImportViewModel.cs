using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// Import from anvisa view model
    /// </summary>
    public class AnvisaImportViewModel
    {
        [Display(Name = "Medicine")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? AnvisaId { get; set; }

        [Display(Name = "Medicine")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string AnvisaText { get; set; }
    }
}