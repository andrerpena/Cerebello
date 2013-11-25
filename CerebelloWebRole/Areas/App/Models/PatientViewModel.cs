using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PatientViewModel : PersonViewModel
    {
        public PatientViewModel()
        {
            this.Sessions = new List<SessionViewModel>();
            this.FutureAppointments = new List<AppointmentViewModel>();
        }

        [Display(Name = "Código")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Code { get; set; }

        [Display(Name = "Convênio")]
        public int? HealthInsuranceId { get; set; }

        [Display(Name = "Convênio")]
        public String HealthInsuranceText { get; set; }

        [Display(Name = "Observações")]
        public String Observations { get; set; }

        [XmlIgnore]
        public List<SessionViewModel> Sessions { get; set; }

        public List<AppointmentViewModel> FutureAppointments { get; set; }

        [XmlIgnore]
        public int PersonId { get; set; }

        [XmlIgnore]
        public int PatientId { get; set; }
    }
}