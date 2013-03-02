using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("PhysicalExamination", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("PhysicalExamination")]
    public class PhysicalExaminationViewModel
    {
        [Display(Name = "Exame físico")]
        public string Notes { get; set; }

        public int? PatientId { get; set; }

        public int? Id { get; set; }
    }
}