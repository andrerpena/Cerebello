using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class MedicinesController : DoctorController
    {
        public MedicineViewModel GetViewModelFromModel(Medicine medicine)
        {
            return new MedicineViewModel
            {
                Id = medicine.Id,
                Name = medicine.Name,
                Usage = medicine.Usage,
                ActiveIngredients = (from activeIngredient in medicine.ActiveIngredients
                                     select new MedicineActiveIngredientViewModel()
                                         {
                                             ActiveIngredientId = activeIngredient.Id,
                                             ActiveIngredientName = activeIngredient.Name
                                         }).ToList(),
                Leaflets = (from leaflet in medicine.Leaflets
                            select new MedicineLeafletViewModel()
                            {
                                Id = leaflet.Id,
                                Description = leaflet.Description,
                                Url = leaflet.Url
                            }).ToList(),
                LaboratoryId = medicine.Laboratory.Id,
                LaboratoryName = medicine.Laboratory.Name
            };
        }

        //
        // GET: /App/Medicines/

        public ActionResult Index()
        {
            var model = new MedicinesIndexViewModel();
            model.LastRegisteredMedicines = (from medicine in db.Medicines.Where(m => m.Doctor.Id == this.Doctor.Id).OrderBy(m => m.Name).Take(5).ToList()
                                             select this.GetViewModelFromModel(medicine)).ToList();
            model.TotalMedicinesCount = db.Medicines.Count();
            return View(model);
        }

        public ActionResult Details(int id)
        {
            if (string.IsNullOrEmpty(ViewBag.DetailsAction as String))
                this.ViewBag.DetailsView = "DetailsViewLeaflets";

            var medicine = db.Medicines.Where(m => m.Id == id).First();
            var model = this.GetViewModelFromModel(medicine);

            return View(model);
        }

        public ActionResult DetailsReceipt(int id)
        {
            this.ViewBag.DetailsView = "DetailsViewLeaflets";
            return this.Details(id);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult Create(MedicineViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? anvisaId = null)
        {
            MedicineViewModel viewModel = new MedicineViewModel();

            if (id != null)
            {
                var medicine = this.db.Medicines.First(m => m.Id == id);
                viewModel = this.GetViewModelFromModel(medicine);
                ViewBag.Title = "Alterando medicamento: " + viewModel.Name;
            }
            else
                ViewBag.Title = "Novo medicamento";

            if (anvisaId != null)
            {
                var sysMedicine = this.db.SYS_Medicine.FirstOrDefault(sm => sm.Id == anvisaId);

                viewModel.Name = sysMedicine.Name;


                // verify if there's already this lab in the user database
                var existingLab = this.db.Laboratories.FirstOrDefault(l => l.Name == sysMedicine.Laboratory.Name && l.DoctorId == this.Doctor.Id);
                viewModel.LaboratoryName = sysMedicine.Laboratory.Name;
                if (existingLab != null)
                    viewModel.LaboratoryId = existingLab.Id;

                viewModel.ActiveIngredients.Clear();
                foreach (var activeIngredient in sysMedicine.ActiveIngredients)
                    viewModel.ActiveIngredients.Add(new MedicineActiveIngredientViewModel() { ActiveIngredientName = activeIngredient.Name });

                viewModel.Leaflets.Clear();
                foreach (var leafleft in sysMedicine.Leaflets)
                    viewModel.Leaflets.Add(new MedicineLeafletViewModel() { Description = leafleft.Description, Url = leafleft.Url });
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(MedicineViewModel formModel)
        {
            // add validation error when Laboratory Id is invalid
            Laboratory laboratory = null;
            if (formModel.LaboratoryId != null)
            {
                laboratory = this.db.Laboratories.FirstOrDefault(lab => lab.Id == formModel.LaboratoryId && lab.DoctorId == this.Doctor.Id);
                if (laboratory == null)
                    this.ModelState.AddModelError<MedicineViewModel>((model) => model.LaboratoryId, "O laboratório informado é inválido");
            }

            // add validation error when any Active Ingredient Id is invalid
            foreach (var activeIngredientViewModel in formModel.ActiveIngredients)
                if (!this.db.ActiveIngredients.Any(ai => ai.Id == activeIngredientViewModel.ActiveIngredientId))
                    this.ModelState.AddModelError<MedicineViewModel>(
                        model => model.ActiveIngredients, string.Format("O princípio ativo '{0}' é inválido", activeIngredientViewModel.ActiveIngredientName));

            if (this.ModelState.IsValid)
            {
                Medicine medicine = null;

                if (formModel.Id != null)
                    medicine = this.db.Medicines.First(m => m.Id == formModel.Id);
                else
                {
                    medicine = new Medicine { PracticeId = this.DbUser.PracticeId, };
                    this.db.Medicines.AddObject(medicine);
                }

                medicine.Name = formModel.Name;
                medicine.Usage = (short)formModel.Usage;
                medicine.Doctor = this.Doctor;

                if (formModel.LaboratoryId != null)
                    medicine.Laboratory = laboratory;

                // Active ingredients (as it's a NxM association, there's no update here, just creates and deletes)
                {
                    // Verify whether any existing active ingredient should be REMOVED.
                    var activeIngredientsDeathQueue = new Queue<ActiveIngredient>();
                    foreach (var existingActiveIngredient in medicine.ActiveIngredients)
                        if (formModel.ActiveIngredients.All(a => a.ActiveIngredientId != existingActiveIngredient.Id))
                            activeIngredientsDeathQueue.Enqueue(existingActiveIngredient);
                    while (activeIngredientsDeathQueue.Any())
                        medicine.ActiveIngredients.Remove(activeIngredientsDeathQueue.Dequeue());

                    // Verify whether any new active ingredient should be ADDED
                    foreach (var activeIngredientViewModel in formModel.ActiveIngredients)
                        if (medicine.ActiveIngredients.All(ai => ai.Id != activeIngredientViewModel.ActiveIngredientId))
                        {
                            // this First has a very small chance of triggering an exception. Before the 'if (this.ModelState.IsValid)'
                            // all active ingredients 
                            var activeIngredient =
                                this.db.ActiveIngredients.First(ai => ai.Id == activeIngredientViewModel.ActiveIngredientId);
                            medicine.ActiveIngredients.Add(activeIngredient);
                        }
                }

                // Leaflets
                {
                    // Verify whether any existing leaflet must be REMOVED
                    var leafletsDeathQueue = new Queue<Leaflet>();
                    foreach (var existingLeaflet in medicine.Leaflets)
                        if (formModel.Leaflets.All(l => l.Id != existingLeaflet.Id))
                            leafletsDeathQueue.Enqueue(existingLeaflet);
                    while (leafletsDeathQueue.Any())
                        this.db.Leaflets.DeleteObject(leafletsDeathQueue.Dequeue());

                    // Verify whether any leaflet should be UPDATED or ADDED
                    foreach (var leafletViewModel in formModel.Leaflets)
                    {
                        var existingLeaftlet = medicine.Leaflets.SingleOrDefault(l => l.Id == leafletViewModel.Id);
                        if (existingLeaftlet == null)
                        {
                            // ADD when not existing
                            medicine.Leaflets.Add(
                                new Leaflet()
                                    {
                                        PracticeId = this.Practice.Id,
                                        Url = leafletViewModel.Url,
                                        Description = leafletViewModel.Description
                                    });
                        }
                        else
                        {
                            // UPDATE when existing
                            existingLeaftlet.Url = leafletViewModel.Url;
                            existingLeaftlet.Description = leafletViewModel.Description;
                        }
                    }
                }

                db.SaveChanges();

                return Redirect(Url.Action("details", new { id = medicine.Id }));
            }

            return View("Edit", formModel);
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            var medicine = this.db.Medicines.First(m => m.Id == id);
            try
            {
                this.db.Medicines.DeleteObject(medicine);
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult AutocompleteLaboratories(string term, int pageSize, int pageIndex)
        {
            var baseQuery = this.db.Laboratories.Where(l => l.DoctorId == this.Doctor.Id);

            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(l => l.Name.Contains(term));

            var query = from l in baseQuery.OrderBy(l => l.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                        orderby l.Name
                        select new AutocompleteRow
                        {
                            Id = l.Id,
                            Value = l.Name
                        };

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(query.ToList()),
                Count = query.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AutocompleteActiveIngredients(string term, int pageSize, int pageIndex)
        {
            var baseQuery = this.db.ActiveIngredients.Where(ai => ai.DoctorId == this.Doctor.Id);

            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(l => l.Name.Contains(term));

            var query = from l in baseQuery.OrderBy(l => l.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                        orderby l.Name
                        select new AutocompleteRow
                        {
                            Id = l.Id,
                            Value = l.Name
                        };

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(query.ToList()),
                Count = query.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActiveIngredientEditor(MedicineActiveIngredientViewModel viewModel)
        {
            return View(viewModel);
        }

        [HttpGet]
        public JsonResult LookupMedication(string term, int pageSize, int pageIndex)
        {
            var baseQuery = this.db.Medicines.Where(m => m.DoctorId == this.Doctor.Id);
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(m => m.Name.Contains(term) || m.ActiveIngredients.Any(ai => ai.Name.Contains(term)));

            var query = from m in baseQuery
                        orderby m.Name
                        select new MedicineLookupGridModel
                        {
                            Id = m.Id,
                            Name = m.Name,
                            LaboratoryName = m.Laboratory.Name
                        };

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()),
                Count = query.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LookupSysMedicine(string term, int pageSize, int pageIndex)
        {
            var baseQuery = this.db.SYS_Medicine.Include("Laboratory").AsQueryable();
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(m => m.Name.Contains(term) || m.ActiveIngredients.Any(ai => ai.Name.Contains(term)));

            var rows = (from p in baseQuery.OrderBy(p => p.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                        select new SysMedicineLookupGridModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            LaboratoryName = p.Laboratory.Name
                        }).ToList();

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(rows),
                Count = baseQuery.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LeafletEditor(MedicineLeafletViewModel viewModel)
        {
            return View(viewModel);
        }

        public ActionResult AnvisaImportEditor(int? medicationId)
        {
            ViewBag.MedicationId = medicationId;
            return View();
        }

        /// <summary>
        /// Searchs for medicines
        /// </summary>
        /// <remarks>
        /// Requirements:
        ///     - Should return a slice of the existing records matching the criteria corresponding to the specified 'Page' 
        ///     - Should return a result correspoding to all records when no search term is provided (respecting pagination)
        ///     - In the result, the 'Count' property should consider all records
        ///     - In the result, the list should bring only results corresponding to the specified page
        /// </remarks>
        public ActionResult Search(SearchModel searchModel)
        {
            var model = new SearchViewModel<MedicineViewModel>();

            var query = from medicine in db.Medicines
                        where medicine.DoctorId == this.Doctor.Id
                        select medicine;

            if (!string.IsNullOrEmpty(searchModel.Term))
                query = from medicine in query where medicine.Name.Contains(searchModel.Term) select medicine;

            // 1-based page index
            var pageIndex = searchModel.Page;
            var pageSize = Constants.GRID_PAGE_SIZE;

            model.Count = query.Count();
            model.Objects = (from m in query
                             select new MedicineViewModel()
                             {
                                 Id = m.Id,
                                 Name = m.Name,
                                 LaboratoryName = m.Laboratory.Name
                             }).OrderBy(p => p.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return View(model);
        }
    }
}
