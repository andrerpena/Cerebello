using System;
using System.ComponentModel.DataAnnotations;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class AppointmentViewModel
    {
        public int? Id { get; set; }

        /// <summary>
        /// Local time converted date
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Record date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime LocalDateTime { get; set; }

        /// <summary>
        /// Local time converted start
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "Start")]
        [TimeDataType]
        public string Start { get; set; }

        /// <summary>
        /// Local time converted end
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Display(Name = "End")]
        [TimeDataType]
        public string End { get; set; }

        [Display(Name = "Record date")]
        public String LocalDateTimeSpelled { get; set; }

        public bool PatientFirstAppointment { get; set; }
        
        [Display(Name = "Patient")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? PatientId { get; set; }

        [Display(Name = "Patient")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String PatientNameLookup { get; set; }

        [Display(Name = "Patient")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public String PatientName { get; set; }

        [Display(Name = "E-mail")]
        [Code.EmailAddress]
        public string PatientEmail { get; set; }

        [Display(Name = "Gender")]
        [EnumDataType(typeof(TypeGender))]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? PatientGender { get; set; }

        // todo: this property is never being set.
        [Display(Name = "Date of birth")]
        [DateOfBirth]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
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
        [Display(Name = "Description")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Description { get; set; }

        /// <summary>
        /// Validation state for date and time parameters.
        /// </summary>
        public DateAndTimeValidationState DateAndTimeValidationState { get; set; }

        /// <summary>
        /// Appointment status
        /// </summary>
        [Display(Name="Status")]
        [EnumDataType(typeof(TypeAppointmentStatus))]
        public int Status { get; set; }

        /// <summary>
        /// The textual representation of the Status
        /// </summary>
        [Display(Name = "Status")]
        public string StatusText { get; set; }

        [Display(Name = "Land line")]
        [UIHint("Phone")]
        public String PatientPhoneCell { get; set; }

        [Display(Name = "Cell phone")]
        [UIHint("Phone")]
        public String PatientPhoneLand { get; set; }

        [Display(Name = "Health insurance")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public int? HealthInsuranceId { get; set; }

        /// <summary>
        /// The health insurance textual represention. This is used only in viewing
        /// </summary>
        [Display(Name = "Health insurance")]
        public string HealthInsuranceName { get; set; }

        /// <summary>
        /// A calculated property indicating whether the appointment is in the past
        /// This property is only used in viewing
        /// </summary>
        public bool IsInThePast { get; set; }

        /// <summary>
        /// Whether the appointment should be happening now
        /// This property is only used in viewing
        /// </summary>
        public bool IsNow { get; set; }

        /// <summary>
        /// A calculated property indicating whether the patient has arrived. PatientArrived will be true if the Status is set to 
        /// accomplished even though it's still in the future. 
        /// </summary>
        public bool PatientArrived { get; set; }
    }

    public enum DateAndTimeValidationState
    {
        Passed,
        Warning,
        Failed,
    }
}
