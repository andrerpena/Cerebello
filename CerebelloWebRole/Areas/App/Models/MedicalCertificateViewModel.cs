using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Model.Metadata;

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
        [Display(Name="Modelo de atestado")]
        [Tooltip("É necessário escolher um entre os modelos de atestado cadastrados. Ao começar a digitar, o sistema irá sugerir modelos. Caso o modelo em questão não seja encontrado, é possível cadastrar um novo clicando no botão Gerenciar modelos de atestado abaixo.")]
        public int? ModelId { get; set; }

        /// <summary>
        /// Certificate model name
        /// </summary>
        [Display(Name = "Modelo de atestado")]
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

        [Display(Name = "Data de emissão")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que o atestado foi registrado")]
        public DateTime? IssuanceDate { get; set; }
    }
}