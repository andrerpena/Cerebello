using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PatientViewModel : PersonViewModel
    {
        public PatientViewModel()
        {
            this.Sessions = new List<SessionViewModel>();
            this.FutureAppointments = new List<AppointmentViewModel>();
        }

        [Display(Name = "Health insurance")]
        public int? HealthInsuranceId { get; set; }

        [Display(Name = "Health insurance")]
        public String HealthInsuranceText { get; set; }

        [Display(Name = "Notes")]
        public String Notes { get; set; }

        [Display(Name = "Patient record #")]
        public string RecordNumber { get; set; }

        [XmlIgnore]
        public List<SessionViewModel> Sessions { get; set; }

        public List<AppointmentViewModel> FutureAppointments { get; set; }

        [XmlIgnore]
        public int PersonId { get; set; }

        [XmlIgnore]
        public int PatientId { get; set; }
    }
}