using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ConfigDaysOffViewModel
    {
        public class DayOff
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string Description { get; set; }
            public List<DayOff> GroupItems { get; set; }
        }

        public ConfigDaysOffViewModel()
        {
            this.DaysOff = new List<DayOff>();
        }

        public List<DayOff> DaysOff { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Start")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Start { get; set; }

        [Display(Name = "End")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? End { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}