using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Leaflet.Crawler
{
    public class MedicineRaw
    {
        public string ActiveIngredient { get; set; }
        public string Name { get; set; }
        public string Laboratory { get; set; }
        public string Concentration { get; set; }
        public string LeafletType { get; set; }
        public string Category { get; set; }
        public string LeafletUrl { get; set; }
        public DateTime ApprovementDate { get; set; }
    }
}
