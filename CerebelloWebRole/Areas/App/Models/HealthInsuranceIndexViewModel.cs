using System.Collections.Generic;

namespace CerebelloWebRole.Areas.App.Models
{
    public class HealthInsuranceIndexViewModel
    {
        public int Count { get; set; }

        public List<HealthInsuranceViewModel> Objects { get; set; }
    }
}
