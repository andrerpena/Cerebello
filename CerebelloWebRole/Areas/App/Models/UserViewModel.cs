using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using System.Web.Mvc;
using CerebelloWebRole.Code.Validation;

namespace CerebelloWebRole.Areas.App.Models
{
    public class UserViewModel : PersonViewModel
    {
        [Display(Name = "Nome identificador")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [UserNameAttribute]
        public string UserName { get; set; }

        public string ImageUrl { get; set; }

        public string UrlIdentifier { get; set; }

        [Display(Name = "Administrador(a)")]
        public bool IsAdministrador { get; set; }

        [Display(Name = "Secretário(a)")]
        public bool IsSecretary { get; set; }

        [Display(Name = "Médico(a)")]
        public bool IsDoctor { get; set; }

        // Information of this user when he/she is a medic.
        // If IsDoctor is false, these properties have no meaning.

        [Display(Name = "CRM")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicCRM { get; set; }

        [Display(Name = "Especialidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalSpecialty { get; set; }

        [Display(Name = "Conselho médico")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalEntity { get; set; }

        [Display(Name = "UF do conselho")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalEntityJurisdiction { get; set; }
    }
}
