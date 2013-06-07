using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class MedicineLeafletViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Descrição")]
        public String Description { get; set; }

        [Url]
        [Display(Name = "URL")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String Url { get; set; }

        public String ViewerUrl { get; set; }

        public String GoogleDocsUrl { get; set; }

        public int MedicineId { get; set; }
        public string MedicineName { get; set; }

        public string GoogleDocsEmbeddedUrl { get; set; }
    }
}