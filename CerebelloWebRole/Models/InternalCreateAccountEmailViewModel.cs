using Cerebello.Model;

namespace CerebelloWebRole.Models
{
    public class InternalCreateAccountEmailViewModel
    {
        public InternalCreateAccountEmailViewModel(User user, CreateAccountViewModel registrationData)
        {
            this.RegistrationData = registrationData.Clone();
            this.RegistrationData.Password = "[PASSWORD]";
            this.RegistrationData.ConfirmPassword = "[PASSWORD]";

            this.UrlIdentifier = user.Practice.UrlIdentifier;
            this.UserName = user.UserName;
        }

        public CreateAccountViewModel RegistrationData { get; set; }

        public string UrlIdentifier { get; set; }

        public string UserName { get; set; }
    }
}
