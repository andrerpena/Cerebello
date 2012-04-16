using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Models;
using CerebelloWebRole.Code.Security;
using CerebelloWebRole.Areas.App.Controllers;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public class PracticeController : CerebelloController
    {
        /// <summary>
        /// Retorna o usuário atual
        /// </summary>
        /// <returns></returns>
        public User GetCurrentUser()
        {
            var identity = this.User as AuthenticatedPrincipal;
            return (User) db.Users.Where(p => p.Id == identity.Profile.Id).First();
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

            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                var authenticatedPrincipal = filterContext.HttpContext.User as AuthenticatedPrincipal;
                if (authenticatedPrincipal != null)
                {
                    var practiceName = this.RouteData.Values["practice"] as string;

                    var practiceUser = (
                        from pu in this.db.UserPractices
                        where
                            pu.Practice.UrlIdentifier == practiceName && pu.UserId == authenticatedPrincipal.Profile.Id
                        select pu).FirstOrDefault();

                    if (practiceUser != null)
                    {
                        this.Practice = practiceUser.Practice;
                        this.ViewBag.Practice = practiceUser.Practice;
                        this.ViewBag.PracticeName = practiceUser.Practice.Name;
                        return;
                    }
                }
            }
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}