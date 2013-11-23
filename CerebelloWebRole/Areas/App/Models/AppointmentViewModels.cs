using System;
using Cerebello.Model;

namespace CerebelloWebRole.Areas.App.Models
{
    // todo: this class is never used.
    public class CreateAppointmentSimplifiedViewModel
    {
        public DateTime Date { get; set; }
        
        public String PatientName { get; set; }
        public TypeGender PatientGender { get; set; }
        public DateTime PatientDateOfBirth { get; set; }

        public int MyProperty { get; set; }
    }
}