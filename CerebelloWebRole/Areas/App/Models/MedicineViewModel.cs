using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicineViewModel
    {
        public MedicineViewModel()
        {
            this.ActiveIngredients = new List<MedicineActiveIngredientViewModel>();
            this.Leaflets = new List<MedicineLeafletViewModel>();
        }

        public int? Id { get; set; }

        /// <summary>
        /// Wether the user is importing
        /// </summary>
        public bool IsImporting { get; set; }

        [Display(Name = "Medicine")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? AnvisaId { get; set; }

        [Display(Name = "Medicine")]
        public string AnvisaText { get; set; }

        [Display(Name = "Custom name")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string AnvisaCustomText { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string Name { get; set; }

        public int? LaboratoryId { get; set; }

        [Display(Name="Laboratory")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string LaboratoryName { get; set; }

        [Display(Name = "Active principles")]
        public List<MedicineActiveIngredientViewModel> ActiveIngredients { get; set; }
        
        [Display(Name = "Leaflets")]
        public List<MedicineLeafletViewModel> Leaflets { get; set; }

        [Display(Name = "Usage")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeUsage))]
        public int Usage { get; set; }

        [Display(Name = "Prescriptions")]
        public SearchViewModel<PrescriptionViewModel> Prescriptions { get; set; }

        [Display(Name = "Observations")]
        public string Observations { get; set; }
    }
}