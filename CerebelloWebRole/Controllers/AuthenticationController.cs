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

        /// <summary>
        /// Logs the user in or not, based on the informations provided.
        /// URL: http://www.cerebello.com.br/authentication/login
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(LoginViewModel loginModel)
        {
            if (!this.ModelState.IsValid || !SecurityManager.Login(loginModel, db))
            {
                ViewBag.LoginFailed = true;
                return View();
            }

#warning Todo seems to be wrong...
            // TODO: efetuar o login

            if (loginModel.Password == Constants.DEFAULT_PASSWORD)
            {
                return RedirectToAction("changepassword", "users", new { area = "app", practice = loginModel.PracticeIdentifier });
            }
            else
            {
                return RedirectToAction("index", "practicehome", new { area = "app", practice = loginModel.PracticeIdentifier });
            }
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
