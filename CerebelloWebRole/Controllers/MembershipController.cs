using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using System.IO;
using CerebelloWebRole.Code.Security;
using Cerebello.Model;

namespace CerebelloWebRole.Areas.Site.Controllers
{
    public class MembershipController : Controller
    {
        private CerebelloEntities db = new CerebelloEntities();

        /// <summary>
        /// Verify whether a user with the same Email exists
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public ActionResult EmailIsAvailable(string emailAddress)
        {
            var email = (from u in db.Emails where u.Address == emailAddress select u).FirstOrDefault();
            return Json(email == null, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /App/Practices/

        public ActionResult Index()
        {
            var practices = (from p in db.Practices orderby p.CreatedOn select p).ToList();
            return View(practices);
        }


        public ActionResult CreatePractice()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreatePractice(CreatePracticeViewModel model)
        {
            if (ModelState.IsValid)
            {
                Practice practice = new Practice();
                practice.Name = model.Name;
                practice.CreatedOn = DateTime.UtcNow;

                // finding current user
                var userData = this.User as AuthenticatedPrincipal;
                if (userData == null)
                    throw new Exception("User should be logged in");

                UserPractice practiceUser = new UserPractice();
                practiceUser.User = db.Users.Where(u => u.Id == userData.Profile.Id).First();
                practiceUser.Practice = practice;
                
                // xxx está faltando definir a licença

                db.Practices.AddObject(practice);
                db.UserPractices.AddObject(practiceUser);

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                }
            }
            return View();
        }

        [HttpPost]
        public bool UploadPicture()
        {
            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase postedFile = Request.Files[fileName];
                    postedFile.SaveAs(Server.MapPath("~/Uploads/") + Path.GetFileName(postedFile.FileName));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
