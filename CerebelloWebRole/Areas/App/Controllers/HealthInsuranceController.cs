using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class HealthInsuranceController : DoctorController
    {
        public ActionResult Index()
        {
            var model = new HealthInsuranceIndexViewModel
                {
                    Objects = (from m in this.db.HealthInsurances
                                   .Where(m => m.Doctor.Id == this.Doctor.Id)
                                   .OrderBy(m => m.Name).Take(5).ToList()
                               select new HealthInsuranceViewModel
                                   {
                                       Id = m.Id,
                                       Name = m.Name,
                                       FirstTimeValue = m.FirstTimeValue,
                                       ReturnValue = m.ReturnValue,
                                   }).ToList(),
                    Count = this.db.Medicines.Count()
                };
            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult Create(HealthInsuranceViewModel viewModel)
        {
            return this.Edit(viewModel);
        }


        [HttpGet]
        public ActionResult Edit(int? id, int? anvisaId = null)
        {
            var viewModel = new HealthInsuranceViewModel();

            if (id != null)
            {
                var healthInsurance = this.db.HealthInsurances.First(m => m.Id == id);
                viewModel = new HealthInsuranceViewModel
                    {
                        Id = healthInsurance.Id,
                        Name = healthInsurance.Name,
                        ReturnValue = healthInsurance.ReturnValue,
                        FirstTimeValue = healthInsurance.FirstTimeValue,
                    };
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(HealthInsuranceViewModel formModel)
        {
            if (this.ModelState.IsValid)
            {
                HealthInsurance healthInsurance;
                if (formModel.Id != null)
                    healthInsurance = this.db.HealthInsurances.First(m => m.Id == formModel.Id);
                else
                {
                    healthInsurance = new HealthInsurance();
                    this.db.HealthInsurances.AddObject(healthInsurance);
                    healthInsurance.DoctorId = this.Doctor.Id;
                    healthInsurance.PracticeId = this.Doctor.PracticeId;
                }

                healthInsurance.Name = formModel.Name;
                healthInsurance.FirstTimeValue = formModel.FirstTimeValue;
                healthInsurance.ReturnValue = formModel.ReturnValue;

                db.SaveChanges();

                return Redirect(Url.Action("Details", new { id = healthInsurance.Id }));
            }

            return View("Edit", formModel);
        }

        public ActionResult Details(int id)
        {
            var hi = this.db.HealthInsurances.Single(h => h.Id == id);

            var viewModel = new HealthInsuranceViewModel
                {
                    Id = hi.Id,
                    Name = hi.Name,
                    FirstTimeValue = hi.FirstTimeValue,
                    ReturnValue = hi.ReturnValue,
                };

            return this.View(viewModel);
        }

        public ActionResult Search(string term)
        {
            var model = new HealthInsuranceIndexViewModel
                {
                    Objects = (from m in this.db.HealthInsurances
                                   .Where(m => m.Doctor.Id == this.Doctor.Id)
                                   .Where(m => m.Name.Contains(term))
                                   .OrderBy(m => m.Name).Take(5).ToList()
                               select new HealthInsuranceViewModel
                                   {
                                       Id = m.Id,
                                       Name = m.Name,
                                       FirstTimeValue = m.FirstTimeValue,
                                       ReturnValue = m.ReturnValue,
                                   }).ToList(),
                    Count = this.db.Medicines.Count()
                };
            return View("Index", model);
        }

        public JsonResult Delete(int id)
        {
            try
            {
                var hi = this.db.HealthInsurances.First(m => m.Id == id);

                this.db.HealthInsurances.DeleteObject(hi);
                this.db.SaveChanges();

                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}