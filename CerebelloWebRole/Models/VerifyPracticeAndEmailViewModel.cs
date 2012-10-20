using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Models
{
    public class VerifyPracticeAndEmailViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [RegularExpression(@"^(\d+)-([\da-fA-F]{32})$")]
        public string Token { get; set; }

        public string Practice { get; set; }

        public string UserNameOrEmail { get; set; }

        public string Password { get; set; }
    }
}
