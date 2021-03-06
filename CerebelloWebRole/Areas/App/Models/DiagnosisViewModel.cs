﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    [XmlRoot("Diagnosis", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
    [XmlType("Diagnosis")]
    public class DiagnosisViewModel
    {
        public int? Id { get; set; }

        [Required]
        public int? PatientId { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "Diagnóstico")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("É possível usar tando doenças/condições listadas no CID-10 quanto texto livre. Ao começar a digitar, o sistema irá sugerir itens da lista do CID-10. Caso um desses itens seja selecionado, o código CID-10 será associado a este diagnóstico.")]
        public string Cid10Code { get; set; }

        /// <summary>
        /// Cid 10 name
        /// </summary>
        [Display(Name = "Diagnóstico")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Cid10Name { get; set; }

        [Display(Name = "Notas")]
        public string Text { get; set; }

        [Display(Name = "Data de registro")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data em que o diagnóstico foi registrado")]
        public DateTime? MedicalRecordDate { get; set; }
    }
}