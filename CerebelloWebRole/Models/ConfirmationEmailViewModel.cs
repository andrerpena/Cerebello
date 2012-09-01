namespace CerebelloWebRole.Models
{
    public class ConfirmationEmailViewModel
    {
        public string PersonName { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public string PracticeUrlIdentifier { get; set; }
    }
}
