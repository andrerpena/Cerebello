using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ConfigPracticeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeTimeZone))]
        [Display(Name = "Fuso-horário do consultório")]
        public short PracticeTimeZone { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [RegularExpression(@"^\s*[\w\p{P}\d]+(\s+[\w\p{P}\d]+)*\s*$", ErrorMessage = "Nome de consultório precisa conter letras, números, ou alguma pontuação.")]
        [Display(Name = "Nome do consultório")]
        public string PracticeName { get; set; }
    }
}
