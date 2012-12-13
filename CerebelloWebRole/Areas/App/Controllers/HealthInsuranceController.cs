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
                    HealthInsurances = (from m in this.db.HealthInsurances
                                            .Where(m => m.Doctor.Id == this.Doctor.Id)
                                            .OrderBy(m => m.Name).ToList()
                                        select new HealthInsuranceViewModel
                                            {
                                                Id = m.Id,
                                                Name = m.Name,
                                                NewAppointmentValue = m.NewAppointmentValue,
                                                ReturnAppointmentValue = m.ReturnAppointmentValue,
                                                ReturnDaysInterval = m.ReturnTimeInterval,
                                                IsActive = m.IsActive,
                                                IsParticular = m.IsParticular,
                                            }).ToList(),
                    Count = this.db.Medicines.Count()
                };
            return View(model);
        }

        [HttpGet]
        public ActionResult Create(bool isParticular)
        {
            return this.Edit((int?)null, isParticular);
        }

        [HttpPost]
        public ActionResult Create(HealthInsuranceViewModel viewModel)
        {
            return this.Edit(viewModel);
        }


        [HttpGet]
        public ActionResult Edit(int? id, bool isParticular = false)
        {
            var viewModel = new HealthInsuranceViewModel
                {
                    ReturnDaysInterval = 30,
                    IsActive = true,
                    IsParticular = isParticular,
                };

            this.ViewBag.IsEditingOrCreating = id != null ? 'E' : 'C';

            if (id != null)
            {
                var healthInsurance = this.db.HealthInsurances.First(m => m.Id == id);
                viewModel = new HealthInsuranceViewModel
                    {
                        Id = healthInsurance.Id,
                        Name = healthInsurance.Name,
                        ReturnAppointmentValue = healthInsurance.ReturnAppointmentValue,
                        NewAppointmentValue = healthInsurance.NewAppointmentValue,
                        ReturnDaysInterval = healthInsurance.ReturnTimeInterval,
                        IsActive = healthInsurance.IsActive,
                        IsParticular = healthInsurance.IsParticular,
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
                healthInsurance.NewAppointmentValue = formModel.NewAppointmentValue;
                healthInsurance.ReturnAppointmentValue = formModel.ReturnAppointmentValue;
                healthInsurance.ReturnTimeInterval = formModel.ReturnDaysInterval;
                healthInsurance.IsActive = formModel.IsActive;
                healthInsurance.IsParticular = formModel.IsParticular;

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
                    NewAppointmentValue = hi.NewAppointmentValue,
                    ReturnAppointmentValue = hi.ReturnAppointmentValue,
                    ReturnDaysInterval = hi.ReturnTimeInterval,
                    IsActive = hi.IsActive,
                    IsParticular = hi.IsParticular,
                };

            return this.View(viewModel);
        }

        public JsonResult Delete(int id)
        {
            try
            {
                var hi = this.db.HealthInsurances.First(m => m.Id == id);

                if (this.db.Appointments.Any(a => a.HealthInsuranceId == id))
                    return this.Json(
                        new JsonDeleteMessage { success = false, text = "Este convênio está sendo usado e não pode ser removido." },
                        JsonRequestBehavior.AllowGet);

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
