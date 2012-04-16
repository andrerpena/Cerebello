using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using Cerebello.Model;

namespace CerebelloWebRole.Areas.Site.Controllers
{
    public class AuthenticationController : Controller
    {
        private CerebelloEntities db = new CerebelloEntities();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel loginModel)
        {
            if (!ModelState.IsValid || !SecurityManager.Login(loginModel, db))
            {
                ViewBag.LoginFailed = true;
                return View();
            }

            return RedirectToAction("index", "membership");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(CreateAccountViewModel registrationData)
        {
            if (ModelState.IsValid)
            {
                db.Users.AddObject(SecurityManager.CreateUser(registrationData, db));
                db.SaveChanges();
                return RedirectToAction("createaccountcompleted");
            }
            return View();
        }

        public ActionResult CreateAccountCompleted()
        {
            return View();
        }
    }
}
