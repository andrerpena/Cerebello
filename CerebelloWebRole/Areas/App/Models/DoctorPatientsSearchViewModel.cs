using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class DoctorPatientsSearchViewModel
    {
        public int Count { get; set; }
        public List<PatientViewModel> Patients { get; set; }
    }
}