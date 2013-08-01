using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class HealthInsuranceViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [StringLength(Constants.DB_NAME_MAX_LENGTH, ErrorMessageResourceName = "MaxLengthValidationMessage")]
        public string Name { get; set; }

        [Display(Name = "Medical appointment cost")]
        public decimal? NewAppointmentValue { get; set; }

        [Display(Name = "Returning medical appointment cost")]
        public decimal? ReturnAppointmentValue { get; set; }

        [Display(Name = "Return interval in days", Description = "Interval in which, if the patient returns, the consultation is considered a return")]
        public int? ReturnDaysInterval { get; set; }

        [Display(Name = "Active", Description = "Indicates whether this health insurance is active")]
        public bool IsActive { get; set; }

        [Display(Name = "Private pay", Description = "Indicates that the consultation is payed by the patient")]
        public bool IsParticular { get; set; }
    }
}
