using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PracticeHomeIndexViewModel
    {
        public List<DoctorViewModel> Doctors { get; set; }

        [Display(Name = "Total de pacientes")]
        public int PatientsCount { get; set; }
    }
}