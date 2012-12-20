using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cerebello.Model;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DoctorHomeViewModel
    {
        public DoctorHomeViewModel()
        {
            this.TodaysAppointments = new List<AppointmentViewModel>();
        }

        /// <summary>
        /// Doctor's name
        /// </summary>
        [Display(Name = "Nome do médico")]
        public string DoctorName { get; set; }

        /// <summary>
        /// Next appointments
        /// </summary>
        public List<AppointmentViewModel> TodaysAppointments { get; set; }

        public TypeGender Gender { get; set; }

        /// <summary>
        /// Next doctor's free time
        /// </summary>
        public Tuple<DateTime, DateTime> NextFreeTime { get; set; }
    }
}