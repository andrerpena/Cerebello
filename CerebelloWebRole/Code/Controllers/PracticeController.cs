using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code
{
    public abstract class PracticeController : CerebelloController
    {
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