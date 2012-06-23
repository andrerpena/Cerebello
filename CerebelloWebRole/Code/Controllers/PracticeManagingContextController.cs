using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Models;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Controllers
{
#warning This class is not being used.
    [RequiresAuthentication]
    public class PracticeManagingContextController : Controller
    {
        public CerebelloEntities db = new CerebelloEntities();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            db.Dispose();
        }
    }
}