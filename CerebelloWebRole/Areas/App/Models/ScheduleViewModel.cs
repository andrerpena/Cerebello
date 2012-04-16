using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ScheduleViewModel
    {
        /// <summary>
        /// Id do médico dono da agenda em questão
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// Time, in minutes, of an appointment
        /// </summary>
        public int SlotMinutes { get; set; }

        public string MinTime { get; set; }

        public string MaxTime { get; set; }

        public bool Weekends { get; set; }
    }
}