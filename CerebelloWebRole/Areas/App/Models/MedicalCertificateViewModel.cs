using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// A receipt prescripted to a patient
    /// </summary>
    [XmlRoot("MedicalCertificate", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("MedicalCertificate")]
    public class MedicalCertificateViewModel
    {
        public MedicalCertificateViewModel()
        {
            this.Fields = new List<MedicalCertificateFieldViewModel>();
        }

        public int? Id { get; set; }
        public int? PatientId { get; set; }

        /// <summary>
        /// ModelMedicalCertificate
        /// </summary>
        [Display(Name="Medical certificate model")]
        [Tooltip("It's necessary to choose a model. As you start typing, the system will suggest models. In case the modal is not yet registered, it's possible to register a new one clicking the 'Manage certificate models' below.")]
        public int? ModelId { get; set; }

        /// <summary>
        /// Certificate model name
        /// </summary>
        [Display(Name = "Medical certificate model")]
        public string ModelName { get; set; }

        /// <summary>
        /// Fields in the medical certificate.
        /// </summary>
        /// <remarks>
        /// This is NOT a variable length list
        /// </remarks>
        public List<MedicalCertificateFieldViewModel> Fields { get; set; }

        /// <summary>
        /// The options for certificate model
        /// </summary>
        public List<SelectListItem> ModelOptions { get; set; }

        [Display(Name = "Issue date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Date this certificate has been issued")]
        public DateTime? IssuanceDate { get; set; }
    }
}