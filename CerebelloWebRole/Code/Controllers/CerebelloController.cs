using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Models;
using Cerebello.Model;
using CerebelloWebRole.Code.Security;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class CerebelloController : Controller
    {
        protected CerebelloEntities db = null;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            // this is because of how tests work
            // if a test controller has been created, this.db has already been populated
            // otherwise let's create a regular one
            if (this.db == null)
                this.db = new CerebelloEntities();

            base.Initialize(requestContext);
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
    }
}