using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Code.Data
{
    /// <summary>
    /// This is user information that is present in all application Controllers and in all Views
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// User Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User Display Name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gravatar Email Hash
        /// </summary>
        public string GravatarEmailHash { get; set; }

        /// <summary>
        /// The doctor's id, if it's a doctor
        /// </summary>
        public int? DoctorId { get; set; }

        /// <summary>
        /// The doctor's URL identifieer, if it's a doctor
        /// </summary>
        public string DoctorUrlIdentifier { get; set; }
    }
}