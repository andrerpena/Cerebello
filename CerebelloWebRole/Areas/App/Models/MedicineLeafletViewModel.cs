using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicineLeafletViewModel
    {
        public int? Id { get; set; }
        public String Description { get; set; }

        [Required]
        public String Url { get; set; }

        public String ViewerUrl { get; set; }

        public String GoogleDocsUrl { get; set; }

        public int MedicineId { get; set; }
        public string MedicineName { get; set; }

        public string GoogleDocsEmbeddedUrl { get; set; }
    }
}