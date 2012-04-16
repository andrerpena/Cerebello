using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    public class EmailViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Endereço")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.EmailAddress)]
        public String Address { get; set; }
    }
}