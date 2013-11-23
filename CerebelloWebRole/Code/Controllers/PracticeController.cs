using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public abstract class PracticeController : CerebelloController
    {
        /// <summary>
        /// Consultório atual
        /// </summary>
        public Practice DbPractice { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            // the base of this class already initializes:
            // - the database InitDb method
            // - the user InitDbUser method
            base.Initialize(requestContext);

            // initializes the practice
            this.InitDbPractice(requestContext);

            if (this.DbPractice != null)
            {
                // Settings practice related view bag values.
                this.ViewBag.Practice = this.DbPractice;
                this.ViewBag.PracticeName = this.DbPractice.Name;

                // Setting a common ViewBag value.
                this.ViewBag.LocalNow = this.GetPracticeLocalNow();
            }
        }

        internal void InitDbPractice(RequestContext requestContext)
        {
            if (this.DbPractice == null && this.DbUser != null)
            {
                if (!requestContext.HttpContext.Request.IsAuthenticated)
                    return;

                var authenticatedPrincipal = requestContext.HttpContext.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception(
                        "HttpContext.User should be a AuthenticatedPrincipal when the user is authenticated");

                var practiceName = this.RouteData.Values["practice"] as string;

                var practice = this.db.Users
                    .Where(u => u.Id == this.DbUser.Id && u.Practice.UrlIdentifier == practiceName)
                    .Select(u => u.Practice)
                    .SingleOrDefault();

                this.DbPractice = practice;
            }
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // reference:
            // if someday we have problems with caching restricted-access pages, the following could be useful:
            // http://farm-fresh-code.blogspot.com.br/2009/11/customizing-authorization-in-aspnet-mvc.html

            base.OnAuthorization(filterContext);

            // if the base has already set a result, then we just exit this method
            if (filterContext.Result != null)
                return;

            // commented: Debug.Assert(this.DbUser != null);
            // reason: the controller methods are always called before all filters
            // so if the user is null here, it means that it is the first time we
            // have a chance to handle this.

            if (this.DbPractice == null)
            {
                filterContext.Result = new UnauthorizedResult();
                return;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // if the base has already set a result, then we just exit this method
            if (filterContext.Result != null)
                return;

            Debug.Assert(this.DbUser != null, "this.DbUser must not be null");
            Debug.Assert(this.DbPractice != null, "this.DbPractice must not be null");

            // Redirect to VerifyPracticeAndEmail, if the practice has not been verified yet.
            if (this.DbPractice.VerificationDate == null)
            {
                filterContext.Result = this.RedirectToAction(
                    "CreateAccountCompleted",
                    "Authentication",
                    new { area = "", practice = this.DbPractice.Name, mustValidateEmail = true });

                return;
            }

            // Loading past notifications.
            if (this.DbPractice != null)
            {
                var existingNotifications =
                    this.db.Notifications.Where(n => n.UserToId == this.DbUser.Id && !n.IsClosed)
                        .OrderBy(n => n.CreatedOn)
                        .Select(n => new UntypedNotificationData { Id = n.Id, Type = n.Type, Data = n.Data }).ToList();

                this.ViewBag.PastNotifications = existingNotifications;
                this.ViewBag.IsTrial = this.DbPractice.AccountContract.SYS_ContractType.IsTrial;
            }
        }

        /// <summary>
        /// Converts the specified UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToLocalDateTime(Practice practice, DateTime utcDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            return result;
        }

        /// <summary>
        /// Converts the specified nullable UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToLocalDateTime(Practice practice, DateTime? utcDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            if (utcDateTime == null)
                return null;

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Value, timeZoneInfo);
            return result;
        }

        /// <summary>
        /// Converts the specified UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public DateTime ConvertToLocalDateTime(DateTime utcDateTime)
        {
            if (this.DbPractice == null) throw new Exception("'DbPractice' must not be null.");

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.DbPractice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            return result;
        }

        /// <summary>
        /// Converts the specified nullable UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public DateTime? ConvertToLocalDateTime(DateTime? utcDateTime)
        {
            if (utcDateTime.HasValue)
                return this.ConvertToLocalDateTime(utcDateTime.Value);

            return null;
        }

        /// <summary>
        /// Converts the specified date and time at the location of the current practice to UTC.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="practiceDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcDateTime(Practice practice, DateTime practiceDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            return DateTimeHelper.ConvertToUtcDateTime(practiceDateTime, timeZoneInfo);
        }

        /// <summary>
        /// Converts the specified date and time at the location of the current practice to UTC.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="practiceDateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToUtcDateTime(Practice practice, DateTime? practiceDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            if (practiceDateTime == null)
                return null;

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            return DateTimeHelper.ConvertToUtcDateTime(practiceDateTime.Value, timeZoneInfo);
        }

        /// <summary>
        /// Converts the specified date and time at the location of the current practice to UTC.
        /// </summary>
        /// <param name="practiceDateTime"></param>
        /// <returns></returns>
        public DateTime ConvertToUtcDateTime(DateTime practiceDateTime)
        {
            if (this.DbPractice == null) throw new Exception("'DbPractice' must not be null.");

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.DbPractice.WindowsTimeZoneId);
            return DateTimeHelper.ConvertToUtcDateTime(practiceDateTime, timeZoneInfo);
        }

        public DateTime GetPracticeLocalNow()
        {
            return ConvertToLocalDateTime(this.DbPractice, this.UtcNowGetter());
        }

        public virtual bool IsSelfUser(User user)
        {
            return false;
        }

        protected Func<DateTime, DateTime> GetToLocalDateTimeConverter()
        {
            return d => ConvertToLocalDateTime(this.DbPractice, d);
        }
    }
}
