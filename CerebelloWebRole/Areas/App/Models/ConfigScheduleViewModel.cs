using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ConfigScheduleViewModel
    {
        public class DayOfWeek {
            public string Name { get; set; }
            public bool IsBusinessDay { get; set; }
            [TimeDataType]
            [Display(Name="Business start")]
            public string WorkdayStartTime { get; set; }
            [TimeDataType]
            [Display(Name = "Business end")]
            public string WorkdayEndTime { get; set; }
            [TimeDataType]
            [Display(Name = "Interval start")]
            public string LunchStartTime { get; set; }
            [TimeDataType]
            [Display(Name = "Interval end")]
            public string LunchEndTime { get; set; }
        }

        public ConfigScheduleViewModel()
        {
            this.DaysOfWeek = new List<DayOfWeek>();
        }

        [Display(Name="Appointment duration")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeAppointmentDuration))]
        public int AppointmentDuration { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }
    }
}