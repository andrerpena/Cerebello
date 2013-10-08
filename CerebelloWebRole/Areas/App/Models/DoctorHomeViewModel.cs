using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cerebello.Model;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DoctorHomeViewModel
    {
        public DoctorHomeViewModel()
        {
            this.TodaysAppointments = new List<AppointmentViewModel>();
        }

        /// <summary>
        /// Doctor's first name
        /// </summary>
        [Display(Name = "Doctor name")]
        public string DoctorFullName { get; set; }
        
        /// <summary>
        /// Next appointments
        /// </summary>
        public List<AppointmentViewModel> TodaysAppointments { get; set; }

        /// <summary>
        /// Next generic appointments
        /// </summary>
        public List<AppointmentViewModel> NextGenericAppointments { get; set; }

        public TypeGender Gender { get; set; }

        /// <summary>
        /// Next doctor's free time
        /// </summary>
        public Tuple<DateTime, DateTime> NextFreeTime { get; set; }

        [Display(Name = "CRM")]
        public string MedicCrm { get; set; }

        [Display(Name = "Specialty")]
        public int? MedicalSpecialtyId { get; set; }

        [Display(Name = "Specialty")]
        public string MedicalSpecialtyName { get; set; }

        [Display(Name = "Medical entity")]
        public int? MedicalEntityId { get; set; }

        [Display(Name = "Medical entity")]
        public string MedicalEntityName { get; set; }

        [Display(Name = "Medical entity state")]
        public int MedicalEntityJurisdiction { get; set; }

        public int NextGenericAppointmentsCount { get; set; }
    }
}