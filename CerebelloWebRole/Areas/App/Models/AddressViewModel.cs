using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AddressViewModel
    {
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