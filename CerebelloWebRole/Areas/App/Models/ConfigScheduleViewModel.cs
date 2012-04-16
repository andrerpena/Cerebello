using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CerebelloWebRole.Models;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CommonLib.Mvc.DataTypes;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ConfigScheduleViewModel
    {
        public class DayOfWeek {
            public string Name { get; set; }
            public bool IsBusinessDay { get; set; }
            [TimeDataType]
            [Display(Name="Horário de início de expediente")]
            public string WorkdayStartTime { get; set; }
            [TimeDataType]
            [Display(Name = "Horário de fim de expediente")]
            public string WorkdayEndTime { get; set; }
            [TimeDataType]
            [Display(Name = "Horário de início de almoço")]
            public string LunchStartTime { get; set; }
            [TimeDataType]
            [Display(Name = "Horário de fim de almoço")]
            public string LunchEndTime { get; set; }
        }

        public ConfigScheduleViewModel()
        {
            this.DaysOfWeek = new List<DayOfWeek>();
        }

        [Display(Name="Duração da consulta")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataType(typeof(TypeAppointmentDuration))]
        public int AppointmentDuration { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }
    }
}