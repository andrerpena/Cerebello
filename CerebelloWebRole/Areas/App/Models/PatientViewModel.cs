using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PatientViewModel : PersonViewModel
    {
        public PatientViewModel()
        {
            this.Sessions = new List<SessionViewModel>();   
        }

        [Display(Name = "Convênio")]
        public int? CoverageId { get; set; }

        [Display(Name = "Convênio")]
        public String CoverageText { get; set; }

        [Display(Name = "Observações")]
        public String Observations { get; set; }

        public List<SessionViewModel> Sessions { get; set; }
    }
}