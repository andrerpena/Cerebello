using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CerebelloWebRole.Models;
using CerebelloWebRole.Code.Security;
using Cerebello.Model;

namespace CerebelloWebRole.Code.Controllers
{
    public abstract class SiteController : Controller
    {
        IPracticeProvider _practiceProvider;
        CerebelloEntities db = new CerebelloEntities();

        public SiteController()
        {
            _practiceProvider = new PracticeProvider(db);
        }

        public SiteController(IPracticeProvider siteProvider)
        {
            _practiceProvider = siteProvider;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            string[] host = requestContext.HttpContext.Request.Headers["Host"].Split(':');
            _practiceProvider.Initialise(host[0]);
            base.Initialize(requestContext);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewData["Site"] = Practice;

            base.OnActionExecuting(filterContext);
        }

        public Practice Practice
        {
            get
            {
                return _practiceProvider.GetCurrentSite();
            }
        }

    }

    public interface IPracticeProvider
    {
        void Initialise(string host);
        Practice GetCurrentSite();
    }

    public class PracticeProvider : IPracticeProvider
    {
        CerebelloEntities _db;
        Practice _practiceModel;

        public PracticeProvider(CerebelloEntities db)
        {
            _db = db;
        }

        public void Initialise(string host)
        {
            _practiceModel = _db.Practices.SingleOrDefault(s => s.UrlIdentifier == host);
        }

        public Practice GetCurrentSite()
        {
            return _practiceModel;
        }
    }

}