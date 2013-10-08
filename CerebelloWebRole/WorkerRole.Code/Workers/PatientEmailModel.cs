using System;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;

namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    public class PatientEmailModel
    {
        public DateTime AppointmentDate;

        public string PatientFirstName { get; set; }

        public string PatientLastName { get; set; }

        public string PracticeUrlId { get; set; }

        public string PracticeName { get; set; }

        public string DoctorFirstName { get; set; }

        public string DoctorLastName { get; set; }

        public string PatientEmail { get; set; }

        public TypeGender PatientGender { get; set; }

        public TypeGender DoctorGender { get; set; }

        public string DoctorPhone { get; set; }

        public string DoctorEmail { get; set; }

        public string PracticeEmail { get; set; }

        public string PracticePhoneMain { get; set; }

        public string PracticePhoneAlt { get; set; }

        public string PracticeSiteUrl { get; set; }

        public string PracticePabx { get; set; }

        public AddressViewModel PracticeAddress { get; set; }
    }
}
