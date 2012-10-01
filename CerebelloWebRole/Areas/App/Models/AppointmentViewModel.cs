using System;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code.Mvc;
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AppointmentViewModel
    {
        public int? Id { get; set; }

        /// <summary>
        /// Local time converted date
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Data da consulta")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Local time converted start
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Início")]
        [TimeDataType]
        public string Start { get; set; }

        /// <summary>
        /// Local time converted end
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Fim")]
        [TimeDataType]
        public string End { get; set; }

        public String DateSpelled { get; set; }

        public bool PatientFirstAppointment { get; set; }

        [Display(Name = "Paciente")]
        public int? PatientId { get; set; }

        [Display(Name = "Paciente")]
        public String PatientNameLookup { get; set; }

        [Display(Name = "Paciente")]
        public String PatientName { get; set; }

        [Display(Name = "Convênio")]
        public int PatientCoverageId { get; set; }

        [Display(Name = "E-mail")]
        [CerebelloWebRole.Code.Mvc.EmailAddress]
        public string PatientEmail { get; set; }

        [Display(Name = "Sexo")]
        [EnumDataType(typeof(TypeGender))]
        public int? PatientGender { get; set; }

        // todo: this property is never being set.
        [Display(Name = "Data de nascimento")]
        [DateOfBirth]
        public DateTime? PatientDateOfBirth { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int DoctorId { get; set; }

        /// <summary>
        /// Validation message displayed next to the time.
        /// </summary>
        public string TimeValidationMessage { get; set; }

        /// <summary>
        /// Indicates whether this is a generic or medical appointement.
        /// </summary>
        public bool IsGenericAppointment { get; set; }

        /// <summary>
        /// Appointment description, when it is a generic appointment, instead of a medical appointment.
        /// </summary>
        [Display(Name = "Descrição")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Description { get; set; }

        /// <summary>
        /// Validation state for date and time parameters.
        /// </summary>
        public DateAndTimeValidationState DateAndTimeValidationState { get; set; }
    }

    public enum DateAndTimeValidationState
    {
        Passed,
        Warning,
        Failed,
    }
}
