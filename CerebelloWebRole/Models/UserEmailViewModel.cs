using Cerebello.Model;

namespace CerebelloWebRole.Models
{
    public class UserEmailViewModel
    {
        public UserEmailViewModel(User user)
        {
            this.UserName = user.UserName;

            if (user.Person != null)
                this.PersonName = user.Person.FullName;

            if (user.Practice != null)
                this.PracticeIdentifier = user.Practice.UrlIdentifier;

            if (user.Practice != null && user.Practice.AccountContract != null)
                this.IsTrial = user.Practice.AccountContract.IsTrial;
        }

        public string PersonName { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// Full token in the format Id-Guid.
        /// </summary>
        public string Token { get; set; }

        public string PracticeIdentifier { get; set; }

        public bool IsTrial { get; set; }
    }
}
