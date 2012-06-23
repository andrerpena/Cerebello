using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using System.Web.Mvc;

namespace CerebelloWebRole.Areas.App.Models
{
    public class UserViewModel : PersonViewModel
    {
        [Display(Name = "Nome identificador")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string UserName { get; set; }

        public string ImageUrl { get; set; }

        public string UrlIdentifier { get; set; }

        [Display(Name = "Administrador(a)")]
        public bool IsAdministrador { get; set; }

        [Display(Name = "Secretário(a)")]
        public bool IsSecretary { get; set; }

        [Display(Name = "Médico(a)")]
        public bool IsMedic { get; set; }

        // Information of this user when he/she is a medic.
        // If IsMedic is false, these properties have no meaning.

        [Display(Name = "CRM")]
        public string MedicCRM { get; set; }

        [Display(Name = "Especialidade")]
        public string MedicalSpecialty { get; set; }

        [Display(Name = "Entidade médica")]
        public string MedicalEntity { get; set; }
    }
}
