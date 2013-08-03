using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicineLaboratoryViewModel
    {
        /// <summary>
        /// Laboratory Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Laboratory Name
        /// </summary>
        [Display(Name = "Name")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Name { get; set; }

        [Display(Name = "Notes")]
        public string Observations { get; set; }

        [Display(Name = "Medicines")]
        public SearchViewModel<MedicineViewModel> Medicines { get; set; }
    }
}