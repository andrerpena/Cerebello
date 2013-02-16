using System;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CerebelloWebRole.Code
{
    public class StatusCodeResult : ActionResult
    {
        public StatusCodeResult(HttpStatusCode statusCode)
            : this(statusCode, null)
        {
        }

        public StatusCodeResult(HttpStatusCode statusCode, string statusDescription)
        {
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;

            this.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            this.Data = new JsonError
            {
                success = false,
                text = statusDescription,
                error = true,
                errorType = this.StatusCode.ToString().ToLowerInvariant(),
                errorMessage = statusDescription,
                status = (int)statusCode,
            };
        }

        public HttpStatusCode StatusCode { get; private set; }
        public string StatusDescription { get; private set; }

        public object Data { get; set; }
        public JsonRequestBehavior JsonRequestBehavior { get; set; }

        public Encoding JsonContentEncoding { get; set; }
        public string JsonContentType { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            response.StatusCode = (int)this.StatusCode;
            if (this.StatusDescription != null)
                response.StatusDescription = this.StatusDescription;

            // if it's an Ajax Request, must return a Json
            if (request.IsAjaxRequest())
            {
                // todo: check for json request: Request.AcceptTypes.Contains("application/json")
                // todo: json requests are being made without setting the content-type to "application/json"
                // todo: Xhr requests made without "application/json" are meant to be XML, not JSON.

                if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                    String.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Json result via GET request is not allowed.");
                }

                this.JsonResult(response);

                // This is an ajax request, so to prevent the FormsAuthentication module
                // redirecting to the login page when the status-code is 401, just end the request.
                // This response will not be cacheable by using aps.net output-caching, because it
                // will be skipped, just like the forms-auth... and any other handler or module
                // that executes at the end of the request (just for 401's though). Read more:
                // http://www.west-wind.com/weblog/posts/2009/May/21/Dont-use-ResponseEnd-with-OutputCache
                if (response.StatusCode == 401)
                    response.End();
            }
        }

        protected void JsonResult(HttpResponseBase response)
        {
            response.ContentType = !string.IsNullOrEmpty(this.JsonContentType) ? this.JsonContentType : "application/json";

            if (this.JsonContentEncoding != null)
                response.ContentEncoding = this.JsonContentEncoding;

            if (this.Data != null)
            {
                var serializer = new JavaScriptSerializer();
                response.Write(serializer.Serialize(this.Data));
            }

        }
    }
}