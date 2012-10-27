using CerebelloWebRole.Code.Security.Principals;

namespace CerebelloWebRole.Code.Security
{
    public class SecurityToken
    {
        public int Salt { get; set; }
        public UserData UserData { get; set; }
    }
}
