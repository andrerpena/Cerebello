using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Cerebello.Model;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Code.Data;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// This is the base Controller for all Controllers inside the App. 
    /// The site and documentation Controllers will NOT be CerebelloController
    /// </summary>
    public class CerebelloController : RootController
    {
        /// <summary>
        /// Object context used throughout all the controller
        /// </summary>
        protected CerebelloEntities db = null;

        public User DbUser { get; private set; }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            this.InitDb();

            // Note that this is not CerebelloController responsability to ensure the user is logged in
            // or that the user exists, simply because there's no way to return anything here in the 
            // Initialize method.
            // The responsability to ensure the user is authenticated and good to go is in the Authorization
            // filter
            if (!this.Request.IsAuthenticated)
                return;

            this.InitDbUser(requestContext);

            if (this.DbUser == null)
                return;

            // this ViewBag will carry user information to the View
            this.ViewBag.UserInfo = new UserInfo()
                {
                    Id = this.DbUser.Id,
                    DisplayName = this.DbUser.Person.FullName,
                    GravatarEmailHash = this.DbUser.Person.EmailGravatarHash,
                    // the following properties will only be set if the current user is a doctor
                    DoctorId = this.DbUser.DoctorId,
                    DoctorUrlIdentifier = this.DbUser.Doctor != null ? this.DbUser.Doctor.UrlIdentifier : null,
                    AdministratorId = this.DbUser.AdministratorId,
                    IsOwner = this.DbUser.IsOwner,
                };
        }

        internal CerebelloEntities InitDb()
        {
            // this is because of how tests work
            // if a test controller has been created, this.db has already been populated
            // otherwise let's create a regular one
            // TODO: tests don't need to populate this property anymore, as CreateNewCerebelloEntities is mockable.
            if (this.db == null)
                this.db = this.CreateNewCerebelloEntities();

            return this.db;
        }

        internal void InitDbUser(RequestContext requestContext)
        {
            if (this.DbUser == null)
            {
                if (!requestContext.HttpContext.Request.IsAuthenticated)
                    return;

                var authenticatedPrincipal = requestContext.HttpContext.User as AuthenticatedPrincipal;

                if (authenticatedPrincipal == null)
                    throw new Exception(
                        "HttpContext.User should be a AuthenticatedPrincipal when the user is authenticated");

                var result = this.db.Users.FirstOrDefault(u => u.Id == authenticatedPrincipal.Profile.Id);

                this.DbUser = result;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            db.Dispose();
        }

        /// <summary>
        /// Returns a view indicating that the requested object does not exist in the database
        /// </summary>
        /// <returns></returns>
        public ActionResult ObjectNotFound()
        {
            // It's necessary to check whether or not it's an AJAX request. 
            // There must be a DIFFERENT output for AJAX and regular requests
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a view indicating that the requested object does not exist in the database (for requests that expect HTML and prints 
        /// the result in the page through AJAX)
        /// </summary>
        /// <returns></returns>
        public ActionResult ObjectNotFound_Ajax()
        {
            return this.View();
        }

        /// <summary>
        /// Database object retrieved using the AccessDbObjectAttribute.
        /// </summary>
        public object DbObject { get; set; }
    }
}
