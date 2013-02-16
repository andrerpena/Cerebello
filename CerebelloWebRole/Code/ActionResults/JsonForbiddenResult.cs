using System.Net;
using System.Web.Mvc;

namespace CerebelloWebRole.Code
{
    public class JsonForbiddenResult : JsonResult
    {
        public JsonForbiddenResult()
            : this(null)
        {
        }

        public JsonForbiddenResult(string statusDescription)
        {
            this.Data = this.Data ?? new JsonError
            {
                success = false,
                text = statusDescription,
                error = true,
                errorType = "forbidden",
                errorMessage = statusDescription,
                status = (int)HttpStatusCode.Forbidden,
            };

            this.StatusDescription = statusDescription;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            base.ExecuteResult(context);

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            if (StatusDescription != null)
                context.HttpContext.Response.StatusDescription = StatusDescription;
        }

        public string StatusDescription { get; private set; }
    }
}