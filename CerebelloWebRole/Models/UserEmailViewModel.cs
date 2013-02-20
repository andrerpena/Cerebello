using Cerebello.Model;

namespace CerebelloWebRole.Models
{
    public class UserEmailViewModel
    {
        public UserEmailViewModel(User user)
        {
            this.PersonName = user.Person.FullName;
            this.UserName = user.UserName;
            this.PracticeIdentifier = user.Practice.UrlIdentifier;
        }

        public string PersonName { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// Full token in the format Id-Guid.
        /// </summary>
        public string Token { get; set; }

        public string PracticeIdentifier { get; set; }
    }
}
