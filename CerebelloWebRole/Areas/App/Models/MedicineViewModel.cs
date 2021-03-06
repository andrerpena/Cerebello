﻿using System.Collections.Generic;
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

        [Display(Name = "Medicamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? AnvisaId { get; set; }

        [Display(Name = "Medicamento")]
        public string AnvisaText { get; set; }

        [Display(Name = "Nome personalizado")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string AnvisaCustomText { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string Name { get; set; }

        public int? LaboratoryId { get; set; }

        [Display(Name="Laboratório")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string LaboratoryName { get; set; }

        [Display(Name = "Princípios ativos")]
        public List<MedicineActiveIngredientViewModel> ActiveIngredients { get; set; }
        
        [Display(Name = "Bulas")]
        public List<MedicineLeafletViewModel> Leaflets { get; set; }

        [Display(Name = "Uso")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeUsage))]
        public int Usage { get; set; }

        [Display(Name = "Prescrições")]
        public SearchViewModel<PrescriptionViewModel> Prescriptions { get; set; }

        [Display(Name = "Observações")]
        public string Observations { get; set; }
    }
}