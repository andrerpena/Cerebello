using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Serialization;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class UserViewModel : PersonViewModel
    {
        [Display(Name = "User name")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [UserName]
        public string UserName { get; set; }

        public string ImageUrl { get; set; }

        [Display(Name = "Manager")]
        public bool IsAdministrador { get; set; }

        [Display(Name = "Desk")]
        public bool IsSecretary { get; set; }

        [Display(Name = "Doctor")]
        public bool IsDoctor { get; set; }

        public bool IsOwner { get; set; }

        // Information of this user when he/she is a medic.
        // If IsDoctor is false, these properties have no meaning.

        // todo: CRM não é válido para outros conselhos, deveria ser "Número no conselho"
        [Display(Name = "Medical entity ID")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicCRM { get; set; }

        [Display(Name = "Medical entity state")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [EnumDataTypeAttribute(typeof(TypeEstadoBrasileiro))]
        [XmlIgnore]
        public int? MedicalEntityJurisdiction { get; set; }

        [Display(Name = "Specialty")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalSpecialtyId { get; set; }

        [Display(Name = "Specialty")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicalSpecialtyName { get; set; }

        [Display(Name = "Specialty profissional")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? MedicalEntityId { get; set; }

        [Display(Name = "Medical entity")]
        public string MedicalEntityName { get; set; }

        [Localizable(true)]
        private const string Secretary = "secretária";

        [Localizable(true)]
        private const string Medic = "médico";

        [Localizable(true)]
        private const string Administrator = "administrador";

        [Localizable(true)]
        private const string Owner = "proprietário da conta";

        [Display(Name = "Roles")]
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
