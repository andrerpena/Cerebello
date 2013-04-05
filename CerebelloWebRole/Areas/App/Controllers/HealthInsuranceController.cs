using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Controller for all health insurance related operations.
    /// </summary>
    public class HealthInsuranceController : DoctorController
    {
        /// <summary>
        /// Gets all health insurances of the current doctor.
        /// </summary>
        /// <returns>ActionResult containing all health insurances.</returns>
        [SelfOrUserRolePermissionAttribute(UserRoleFlags.Administrator)]
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
            return this.View(model);
        }

        /// <summary>
        /// Begins the creation of a new health insurance for the current doctor.
        /// </summary>
        /// <param name="isParticular"> Whether the new insurance is particular or a real health insurance. </param>
        /// <returns> ActionResult containing initial information to create a new insurance. </returns>
        [HttpGet]
        [SelfOrUserRolePermissionAttribute(UserRoleFlags.Administrator)]
        public ActionResult Create(bool isParticular)
        {
            return this.Edit((int?)null, isParticular);
        }

        /// <summary>
        /// Creates a new health insurance using the data passed in the viewModel parameter.
        /// </summary>
        /// <param name="viewModel">Data that is used to create the new health insurance.</param>
        /// <returns> ActionResult containing feedback about the creation. </returns>
        /// <remarks>
        /// Creates a new health insurance using the information passed in the viewModel parameter;
        /// Otherwise, the process is restarted, with already inserted data so that it can be corrected.
        /// </remarks>
        [HttpPost]
        [SelfOrUserRolePermissionAttribute(UserRoleFlags.Administrator)]
        public ActionResult Create(HealthInsuranceViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        [SelfOrUserRolePermissionAttribute(UserRoleFlags.Administrator)]
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

            return this.View("Edit", viewModel);
        }

        [HttpPost]
        [SelfOrUserRolePermissionAttribute(UserRoleFlags.Administrator)]
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

                return this.Redirect(this.Url.Action("Details", new { id = healthInsurance.Id }));
            }

            return this.View("Edit", formModel);
        }

        [SelfOrUserRolePermissionAttribute(UserRoleFlags.Administrator)]
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

        [SelfOrUserRolePermissionAttribute(UserRoleFlags.Administrator)]
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

        public JsonResult LookupHealthInsurances(string term, int pageSize, int? pageIndex)
        {
            var listInsurances = this.Doctor.HealthInsurances
                .Where(hi => hi.IsActive)
                .OrderByDescending(h => h.IsParticular)
                .ThenBy(h => h.Name)
                .ToList();

            IEnumerable<HealthInsurance> baseQuery = listInsurances;

            if (!string.IsNullOrEmpty(term))
            {
                if (pageIndex == null)
                {
                    // if pageIndex is null, locate the first page where the terms can be found
                    var val = listInsurances.Select((hi, idx) => new { hi, idx }).FirstOrDefault(d => d.hi.Name.Contains(term));
                    pageIndex = val != null ? val.idx / pageSize + 1 : 1;
                }
                else
                {
                    baseQuery = baseQuery.Where(l => l.Name.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
                }
            }
            else if (pageIndex == null)
            {
                // if pageIndex is null and there is no term to look for, just go to the first page
                pageIndex = 1;
            }

            var rows = (from p in baseQuery.OrderBy(p => p.Name).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize).ToList()
                        select new
                        {
                            Id = p.Id,
                            Value = p.Name,
                        }).ToList();

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(rows),
                Page = pageIndex.Value,
                Count = baseQuery.Count(),
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
