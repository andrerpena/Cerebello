using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Model.Metadata;

namespace CerebelloWebRole.Areas.App.Models
{
    public class PatientFileViewModel
    {
        public int? Id { get; set; }

        public int? PatientId { get; set; }

        public DateTime CreatedOn { get; set; }

        [Display(Name = "Nome do arquivo")]
        public string FileName { get; set; }

        public string FileContainer { get; set; }

        [Display(Name = "Descrição")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Description { get; set; }

        [Display(Name = "Arquivo")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public HttpPostedFileBase File { get; set; }

        [Display(Name = "Data do arquivo")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data relativa ao arquivo. Por exemplo, se for uma foto, data em que a foto foi tirada")]
        public DateTime? FileDate { get; set; }

        [Display(Name = "Data de recebimento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que o arquivo foi cadastrado")]
        public DateTime? ReceiveDate { get; set; }
    }
}