namespace CerebelloWebRole.Code.Notifications.Data
{
    public class MedicalAppointmentNotificationData
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string Time { get; set; }
        public string DoctorName { get; set; }
        public int DoctorId { get; set; }
        public int AppointmentId { get; set; }
        public string PracticeIdentifier { get; set; }
        public string DoctorIdentifier { get; set; }
    }
}