using System.ComponentModel;
using System.Net;

namespace CerebelloWebRole.Code
{
    public class UnauthorizedResult : StatusCodeResult
    {
        public UnauthorizedResult()
            : this(null)
        {
        }

        public UnauthorizedResult([Localizable(true)] string statusDescription)
            : base(HttpStatusCode.Unauthorized, statusDescription)
        {
        }
    }
}