namespace CerebelloWebRole.Code
{
    public class MedicalAppointmentNotificationData
    {
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string Time { get; set; }
        public string DoctorFullName { get; set; }
        public int DoctorId { get; set; }
        public int AppointmentId { get; set; }
        public string PracticeIdentifier { get; set; }
        public string DoctorIdentifier { get; set; }
    }
}