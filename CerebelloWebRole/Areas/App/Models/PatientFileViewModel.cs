using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CerebelloWebRole.App_GlobalResources;

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

        /// <summary>
        /// This will ALWAYS BE NULL because files cannot be posted via ajax
        /// </summary>
        [Display(Name = "Arquivo")]
        public HttpPostedFileBase PostedFile { get; set; }
    }
}