using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AddressViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Logradouro")]
        public String Street { get; set; }

        [Display(Name = "Complemento")]
        public String Complement { get; set; }

        [Display(Name = "Bairro")]
        public String Neighborhood { get; set; }

        [Display(Name = "Estado")]
        public String StateProvince { get; set; }

        [Display(Name = "Cidade")]
        public String City { get; set; }

        [Display(Name = "CEP")]
        public String CEP { get; set; }
    }
}