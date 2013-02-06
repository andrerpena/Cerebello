using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PhysicalExaminationViewModel
    {
        [Display(Name = "Exame físico")]
        public string Notes { get; set; }

        public int? PatientId { get; set; }

        public int? Id { get; set; }
    }
}