using System.Diagnostics;

namespace Cerebello.Model
{
    [DebuggerDisplay("Id={Id}; UrlIdentifier={UrlIdentifier}; Users={Users.Count}; Owner={Owner.UserName}")]
    public partial class Practice
    {
    }

    [DebuggerDisplay("Id={Id}; UserName={UserName}; Practice={Practice.UrlIdentifier}; {DoctorId != null ? \"D\" : \"\"} {AdministratorId != null ? \"A\" : \"\"} {SecretaryId != null ? \"S\" : \"\"} {IsOwner ? \"Owner\" : \"\"}")]
    public partial class User
    {
    }
}
