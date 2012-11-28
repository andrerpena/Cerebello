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
            this.NextAppointments = new List<AppointmentViewModel>();
        }

        /// <summary>
        /// Doctor's name
        /// </summary>
        [Display(Name = "Nome do médico")]
        public string DoctorName { get; set; }

        /// <summary>
        /// Next appointments
        /// </summary>
        public List<AppointmentViewModel> NextAppointments { get; set; }

        public TypeGender Gender { get; set; }
    }
}