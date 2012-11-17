using System.ComponentModel.DataAnnotations;
using System.Linq;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Validation;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Models
{
    public class UserViewModel : PersonViewModel
    {
        [Display(Name = "Nome identificador")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [UserNameAttribute]
        public string UserName { get; set; }

        public string ImageUrl { get; set; }

        [Display(Name = "Administrador(a)")]
        public bool IsAdministrador { get; set; }

        [Display(Name = "Secretário(a)")]
        public bool IsSecretary { get; set; }

        [Display(Name = "Médico(a)")]
        public bool IsDoctor { get; set; }

        // Information of this user when he/she is a medic.
        // If IsDoctor is false, these properties have no meaning.

        // todo: CRM não é válido para outros conselhos, deveria ser "Número no conselho"
        [Display(Name = "CRM")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicCRM { get; set; }

        [Display(Name = "Estado do conselho")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataTypeAttribute(typeof(TypeEstadoBrasileiro))]
        public int? MedicalEntityJurisdiction { get; set; }

        [Display(Name = "Especialidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalSpecialtyId { get; set; }

        [Display(Name = "Especialidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalSpecialtyName { get; set; }

        // todo: o nome disso deveria ser "Conselho profissional"
        [Display(Name = "Conselho médico")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalEntityId { get; set; }

        [Display(Name = "Conselho médico")]
        public string MedicalEntityName { get; set; }

        public string UserRoles
        {
            get
            {
                var userRoles = new[]
                    {
                        (this.IsSecretary ? "secretária" : ""),
                        (this.IsDoctor ? "médico" : ""),
                        (this.IsAdministrador ? "administrador" : ""),
                    }.Where(ur => ur != "").ToArray();
                var andParts = new[]
                    {
                        string.Join(", ", userRoles.Skip(1)),
                        userRoles.Take(1).SingleOrDefault() ?? ""
                    }.Where(ur => ur != "");
                var result = string.Join(" e ", andParts);
                return result;
            }
        }
    }
}
