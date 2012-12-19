using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PatientsIndexViewModel
    {
        /// <summary>
        /// Data type needed to fill the patient age distribution chart
        /// </summary>
        public class ChartPatientAgeDistribution
        {
            public int Gender { get; set; }
            public int? Age { get; set; }
            public int Count { get; set; }
        }

        /// <summary>
        /// The list of last registered patients
        /// </summary>
        public List<PatientViewModel> LastRegisteredPatients { get; set; }

        /// <summary>
        /// Total number of patients
        /// </summary>
        public int TotalPatientsCount { get; set; }
    }
}