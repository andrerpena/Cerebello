namespace CerebelloWebRole.WorkerRole.Code.Workers
{
    public class PatientEmailModel
    {
        public string PatientName { get; set; }
        public string PracticeUrlId { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
