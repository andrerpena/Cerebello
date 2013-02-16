using System.Net;

namespace CerebelloWebRole.Code
{
    public class UnauthorizedResult : StatusCodeResult
    {
        public UnauthorizedResult(string statusDescription)
            : base(HttpStatusCode.Unauthorized, statusDescription)
        {
        }
    }
}