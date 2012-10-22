using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using System;

namespace CerebelloWebRole.Code
{
    public abstract class PracticeController : CerebelloController
    {
        /// <summary>
        /// Consultório atual
        /// </summary>
        // todo: this property should be name DBPractice like the DBUser property.
        protected Practice Practice { get; set; }

        /// <summary>
        /// Converts the specified UTC date and time for the location of the current practice.
        /// </summary>
        /// <param name="practice"> </param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        protected static DateTime ConvertToLocalDateTime(Practice practice, DateTime utcDateTime)
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
            if (timeZoneInfo.IsInvalidTime(practiceDateTime))
            {
                // Determinando o intervalo inexistente, por conta do
                // adiantamento de horário de verão,
                // que contém a data e hora passada em practiceDateTime.
                // e.g. horário é adiantado no dia 2012/10/21 às 00:00 em 1 hora,
                // então todos os horários no intervalo seguinte são inválidos:
                // [2012/10/21 00:00:00,0000000 ~ 2012/10/21 01:00:00,0000000[
                // O intervalo retornado é exclusivo em ambas as pontas,
                // de forma que as data em ambas as pontas sejam VÁLIDAS.
                // Então o intervalo de exemplo acima será dado assim:
                // ]2012/10/20 23:59:59,9999999 ~ 2012/10/21 01:00:00,0000000[
                var invalidIntervals = timeZoneInfo.GetAdjustmentRules()
                    .Where(ar => ar.DaylightDelta != TimeSpan.Zero)
                    .Select(ar => ar.DaylightDelta > TimeSpan.Zero
                        ? new { Delta = ar.DaylightDelta, Transition = ar.DaylightTransitionStart, Date = ar.DateStart }
                        : new { Delta = -ar.DaylightDelta, Transition = ar.DaylightTransitionEnd, Date = ar.DateStart })
                    .Select(x => new { Start = GetTransitionDateTime(x.Transition, x.Date.Year), x.Delta })
                    .Select(x => new { x.Start, x.Delta, End = (x.Start + x.Delta).AddTicks(-1) })
                    .Select(x => new { StartExclusive = x.Start.AddTicks(-1), EndExclusive = x.End.AddTicks(1) })
                    .Where(x => x.StartExclusive < practiceDateTime && x.EndExclusive > practiceDateTime)
                    .ToList();

                // Deve haver apenas um intervalo.
                var invalidInterval = invalidIntervals.Single();

                // Determinando a ponta do intervalo que está dentro do dia atual, passado em practiceDateTime.
                var dateTime = practiceDateTime.Day == invalidInterval.StartExclusive.Day
                                   ? invalidInterval.StartExclusive
                                   : invalidInterval.EndExclusive;

                // Convertendo a data para UTC.
                var result = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZoneInfo);
                return result;
            }
            else
            {
                var result = TimeZoneInfo.ConvertTimeToUtc(practiceDateTime, timeZoneInfo);
                return result;
            }
        }

        private static DateTime GetTransitionDateTime(TimeZoneInfo.TransitionTime transition, int year)
        {
            DateTime date;
            if (transition.IsFixedDateRule)
            {
                date = new DateTime(year, transition.Month, transition.Day) + transition.TimeOfDay.TimeOfDay;
            }
            else
            {
                date = new DateTime(year, transition.Month, 1);
                date = date.DayOfWeekFromNow(transition.DayOfWeek, transition.Week - 1);
                while (date.Month != transition.Month)
                    date = date.AddDays(-7);
            }
            date = date + transition.TimeOfDay.TimeOfDay;
            return date;
        }

        public DateTime GetPracticeLocalNow()
        {
            return ConvertToLocalDateTime(this.Practice, this.UtcNowGetter());
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // setting up user
            Debug.Assert(this.DbUser != null);

            // setting up practice
            var practiceName = this.RouteData.Values["practice"] as string;
            var controllerName = this.RouteData.Values["controller"] as string;
            var actionName = this.RouteData.Values["action"] as string;

            var practice = this.db.Users
                .Where(u => u.Id == this.DbUser.Id && u.Practice.UrlIdentifier == practiceName)
                .Select(u => u.Practice)
                .SingleOrDefault();

            if (practice == null)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

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

            // Setting a common ViewBag value.
            this.ViewBag.LocalNow = this.GetPracticeLocalNow();
        }
    }
}
