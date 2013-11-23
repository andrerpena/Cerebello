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

        [Display(Name = "Intervalo de retorno em dias", Description = "Intervalo no qual uma consulta subsequente é considerada como sendo retorno pelo convênio médico.")]
        public int? ReturnDaysInterval { get; set; }

        [Display(Name = "Está ativo?", Description = "Indica se novas consultas podem ser marcadas usando este convênio.")]
        public bool IsActive { get; set; }

        [Display(Name = "É particular?", Description = "Indica se é uma forma de atendimento particular, ao invés de um atendimento conveniado.")]
        public bool IsParticular { get; set; }
    }
}
