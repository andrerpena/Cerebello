using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CerebelloWebRole.App_GlobalResources;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicineLaboratoryViewModel
    {
        /// <summary>
        /// Laboratory Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Laboratory Name
        /// </summary>
        [Display(Name = "Nome")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Name { get; set; }
    }
}