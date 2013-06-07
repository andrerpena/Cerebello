using System;
using System.Web.Routing;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// This is the base Controller for all Controllers inside the App.
    /// The site and documentation Controllers will be CerebelloSiteController.
    /// </summary>
    public class CerebelloSiteController : RootController
    {
        /// <summary>
        /// Object context used throughout all the controller
        /// </summary>
        public CerebelloEntitiesAccessFilterWrapper db = null;

        public User DbUser { get; private set; }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            // Note that this is not CerebelloController responsability to ensure the user is logged in
            // or that the user exists, simply because there's no way to return anything here in the
            // Initialize method.
            // The responsability to ensure the user is authenticated and good to go is in the
            // AuthenticationFilter class.
            if (!this.Request.IsAuthenticated)
                return;

            this.InitDbUser(requestContext);

            if (this.DbUser != null)
            {
                // this ViewBag will carry user information to the View
                this.ViewBag.UserInfo = CerebelloController.GetUserInfo(this.DbUser);
            }
        }

        internal CerebelloEntitiesAccessFilterWrapper InitDb()
        {
            // this is because of how tests work
            // if a test controller has been created, this.db has already been populated
            // otherwise let's create a regular one
            return this.db ?? (this.db = new CerebelloEntitiesAccessFilterWrapper(this.CreateNewCerebelloEntities()));
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

                var db1 = this.InitDb();
                var result = db1.SetCurrentUserById(authenticatedPrincipal.Profile.Id);

                this.DbUser = result;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.db != null)
                this.db.Dispose();
        }
    }
}
