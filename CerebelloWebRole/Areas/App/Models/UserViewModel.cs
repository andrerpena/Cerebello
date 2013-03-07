using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;
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

        [Display(Name = "Administrador(a)")]
        public bool IsAdministrador { get; set; }

        [Display(Name = "Secretário(a)")]
        public bool IsSecretary { get; set; }

        [Display(Name = "Médico(a)")]
        public bool IsDoctor { get; set; }

        public bool IsOwner { get; set; }

        // Information of this user when he/she is a medic.
        // If IsDoctor is false, these properties have no meaning.

        // todo: CRM não é válido para outros conselhos, deveria ser "Número no conselho"
        [Display(Name = "Número no conselho profissional")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicCRM { get; set; }

        [Display(Name = "Estado do conselho profissional")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataTypeAttribute(typeof(TypeEstadoBrasileiro))]
        [XmlIgnore]
        public int? MedicalEntityJurisdiction { get; set; }

        [Display(Name = "Especialidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalSpecialtyId { get; set; }

        [Display(Name = "Especialidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalSpecialtyName { get; set; }

        [Display(Name = "Conselho profissional")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalEntityId { get; set; }

        [Display(Name = "Conselho profissional")]
        public string MedicalEntityName { get; set; }

        [Localizable(true)]
        private const string Secretary = "secretária";

        [Localizable(true)]
        private const string Medic = "médico";

        [Localizable(true)]
        private const string Administrator = "administrador";

        [Localizable(true)]
        private const string Owner = "proprietário da conta";

        [Display(Name = "Funções")]
        public string UserRoles
        {
            get
            {
                var userRoles = new[]
                    {
                        (this.IsSecretary ? Secretary : ""),
                        (this.IsDoctor ? Medic : ""),
                        (this.IsAdministrador ? Administrator : ""),
                        (this.IsOwner ? Owner : ""),
                    }.Where(ur => ur != "").ToArray();

                var result = StringHelper.Join(", ", " e ", userRoles);

                return result;
            }
        }
    }
}
