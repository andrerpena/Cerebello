using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Security;
using System;

namespace CerebelloWebRole.Code
{
    public abstract class PracticeController : CerebelloController
    {
        public PracticeController()
        {
        }
        
        /// <summary>
        /// User
        /// </summary>
        public User DBUser { get; private set; }

        /// <summary>
        /// Consultório atual
        /// </summary>
        public Practice Practice { get; private set; }

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
        /// Converts the specified date and time at the location of the current practice to UTC.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="practiceDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcDateTime(Practice practice, DateTime practiceDateTime)
        {
            if (practice == null) throw new ArgumentNullException("practice");

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeToUtc(practiceDateTime, timeZoneInfo);
            return result;
        }

        public DateTime GetPracticeLocalNow()
        {
            return ConvertToLocalDateTime(this.Practice, this.UtcNowGetter());
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // setting up user
            var identity = this.User as AuthenticatedPrincipal;
            this.DBUser = (User)this.db.Users.First(p => p.Id == identity.Profile.Id);

            // setting up practice
            var practiceName = this.RouteData.Values["practice"] as string;
            var controllerName = this.RouteData.Values["controller"] as string;
            var actionName = this.RouteData.Values["action"] as string;

            var practice = this.db.Users
                .Where(u => u.Id == this.DBUser.Id && u.Practice.UrlIdentifier == practiceName)
                .Select(u => u.Practice)
                .SingleOrDefault();

            if (practice == null)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }
            else
            {
                this.Practice = practice;
                this.ViewBag.Practice = practice;
                this.ViewBag.PracticeName = practice.Name;

                // Redirect to VerifyPracticeAndEmail, if the practice has not been verified yet.
                if (practice.VerificationDate == null)
                {
                    filterContext.Result = this.RedirectToAction("VerifyPracticeAndEmail", "Authentication", new { area = "", practice = practiceName });
                    return;
                }

                // Redirect to welcome screen if it was not presented yet.
                if (this.Practice.ShowWelcomeScreen
                    && !(controllerName.ToLowerInvariant() == "practicehome"
                        && actionName.ToLowerInvariant() == "welcome"))
                {
                    filterContext.Result = this.RedirectToAction("Welcome", "PracticeHome", new { area = "App", practice = practiceName });
                    return;
                }
            }

            // Setting a common ViewBag value.
            this.ViewBag.LocalNow = this.GetPracticeLocalNow();
        }
    }
}
