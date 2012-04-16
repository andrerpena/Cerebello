using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PatientsIndexViewModel
    {
        public class ChartPatientAgeDistribution
        {
            public int Gender { get; set; }
            public int? Age { get; set; }
            public int Count { get; set; }
        }

        public List<ChartPatientAgeDistribution> PatientAgeDistribution { get; set; }

        public List<PatientViewModel> Patients { get; set; }
        public int Count { get; set; }
    }
}