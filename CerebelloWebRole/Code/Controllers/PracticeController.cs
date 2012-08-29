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
        /// Retorna o usuário atual
        /// </summary>
        /// <returns></returns>
        public User GetCurrentUser()
        {
            var identity = this.User as AuthenticatedPrincipal;
            return (User)db.Users.Where(p => p.Id == identity.Profile.Id).First();
        }

        public int GetCurrentUserId()
        {
            var identity = this.User as AuthenticatedPrincipal;
            return identity.Profile.Id;
        }

        /// <summary>
        /// Consultório atual
        /// </summary>
        public Practice Practice { get; private set; }

        /// <summary>
        /// Converts the specified UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToLocalDateTime(Practice practice, DateTime utcDateTime)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            return result;
        }

        /// <summary>
        /// Converts the specified date and time at the location of the current practice to UTC.
        /// </summary>
        /// <param name="practiceDateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToUtcDateTime(Practice practice, DateTime practiceDateTime)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(practice.WindowsTimeZoneId);
            var result = TimeZoneInfo.ConvertTimeToUtc(practiceDateTime, timeZoneInfo);
            return result;
        }

        public DateTime GetPracticeLocalNow()
        {
            return ConvertToLocalDateTime(this.Practice, this.UtcNowGetter());
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Setting a common ViewBag value.
            this.ViewBag.LocalNow = this.GetPracticeLocalNow();

            base.OnActionExecuted(filterContext);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var practiceName = this.RouteData.Values["practice"] as string;

            var userId = this.GetCurrentUserId();
            var practice = this.db.Users.Where(u => u.Id == userId).First().Practice;

            this.Practice = practice;
            this.ViewBag.Practice = practice;
            this.ViewBag.PracticeName = practice.Name;
        }
    }
}
