namespace CerebelloWebRole.Code
{
    public class PatientArrivedNotificationData
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string Time { get; set; }
        public string PracticeIdentifier { get; set; }
        public string DoctorIdentifier { get; set; }
    }
}