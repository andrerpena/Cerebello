using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class HealthInsuranceViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string Name { get; set; }

        [Display(Name = "Valor da nova consulta")]
        public decimal? NewAppointmentValue { get; set; }

        [Display(Name = "Valor da consulta de retorno")]
        public decimal? ReturnAppointmentValue { get; set; }

        [Display(Name = "Intervalo de retorno em dias", Description = "Intervalo no qual uma consulta subsequente � considerada como sendo retorno pelo conv�nio m�dico.")]
        public int? ReturnDaysInterval { get; set; }

        [Display(Name = "Est� ativo?", Description = "Intervalo no qual uma consulta subsequente � considerada como sendo retorno pelo conv�nio m�dico.")]
        public bool IsActive { get; set; }
    }
}
