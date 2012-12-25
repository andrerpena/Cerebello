using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Filters;
using JetBrains.Annotations;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [SelfOrUserRolePermission(UserRoleFlags.Administrator)]
    public class MedicinesController : DoctorController
    {
        private MedicineLeafletViewModel GetLeafletViewModel([NotNull] Leaflet leaflet)
        {
            if (leaflet == null) throw new ArgumentNullException("leaflet");

            Func<string, bool> canLeafletBeVisualized = url => url.ToLower().EndsWith(".pdf") ||
                                                               url.ToLower().EndsWith(".doc") ||
                                                               url.ToLower().EndsWith(".docx") ||
                                                               url.ToLower().EndsWith(".txt");

            return new MedicineLeafletViewModel()
                {
                    Id = leaflet.Id,
                    Description = leaflet.Description,
                    Url = leaflet.Url,
                    MedicineId = leaflet.Medicines.First().Id,
                    MedicineName = leaflet.Medicines.First().Name,
                    ViewerUrl = canLeafletBeVisualized(leaflet.Url)
                                    ? Url.Action("ViewLeaflet", new { id = leaflet.Id })
                                    : null,
                    GoogleDocsUrl = canLeafletBeVisualized(leaflet.Url)
                                        ? string.Format("http://docs.google.com/viewer?url={0}", leaflet.Url)
                                        : null,
                    GoogleDocsEmbeddedUrl = canLeafletBeVisualized(leaflet.Url)
                    ? string.Format("http://docs.google.com/viewer?embedded=true&url={0}", leaflet.Url)
                    : null,
                };
        }

        public MedicineViewModel GetViewModelFromModel(Medicine medicine, int? page = null)
        {
            if (!page.HasValue)
                page = 1;
            var pageSize = Constants.GRID_PAGE_SIZE;

            var prescriptionsQuery = from receiptMedicine in medicine.ReceiptMedicines
                                     let patient = receiptMedicine.Receipt.Patient
                                     orderby receiptMedicine.Receipt.CreatedOn descending
                                     select new PrescriptionViewModel()
                                         {
                                             PatientId = patient.Id,
                                             PatientName = patient.Person.FullName,
                                             Prescription = receiptMedicine.Prescription,
                                             Quantity = receiptMedicine.Quantity,
                                             Date = receiptMedicine.Receipt.CreatedOn
                                         };

            return new MedicineViewModel
            {
                Id = medicine.Id,
                Name = medicine.Name,
                Usage = medicine.Usage,
                Observations = medicine.Observations,
                ActiveIngredients = (from activeIngredient in medicine.ActiveIngredients
                                     select new MedicineActiveIngredientViewModel()
                                         {
                                             Id = activeIngredient.Id,
                                             Name = activeIngredient.Name
                                         }).ToList(),
                Leaflets = (from leaflet in medicine.Leaflets
                            select this.GetLeafletViewModel(leaflet)
                            ).ToList(),
                Prescriptions = new SearchViewModel<PrescriptionViewModel>()
                    {
                        Objects = prescriptionsQuery.Skip((page.Value - 1) * pageSize).Take(pageSize).ToList(),
                        Count = prescriptionsQuery.Count()
                    },
                LaboratoryId = medicine.Laboratory.Id,
                LaboratoryName = medicine.Laboratory.Name
            };
        }

        public ActionResult ViewLeaflet(int id)
        {
            var leaflet = this.db.Leaflets.FirstOrDefault(l => l.Id == id);
            if (leaflet == null)
                return this.ObjectNotFound();

            var viewModel = this.GetLeafletViewModel(leaflet);
            return this.View(viewModel);
        }

        //
        // GET: /App/Medicines/

        public ActionResult Index()
        {
            var viewModel = new MedicinesIndexViewModel
                {
                    LastRegisteredMedicines =
                        (from medicine in this.db.Medicines.Where(m => m.Doctor.Id == this.Doctor.Id).OrderByDescending(m => m.CreatedOn).Take(Constants.LAST_REGISTERED_OBJECTS_COUNT).ToList()
                         select this.GetViewModelFromModel(medicine)).ToList(),
                    TotalMedicinesCount = this.db.Medicines.Count()
                };
            return View(viewModel);
        }

        public ActionResult Details(int id, int? page)
        {
            if (string.IsNullOrEmpty(ViewBag.DetailsAction as String))
                this.ViewBag.DetailsView = "DetailsViewLeaflets";

            var medicine = this.db.Medicines.First(m => m.Id == id);
            var model = this.GetViewModelFromModel(medicine, page);

            return View(model);
        }

        public ActionResult DetailsReceipt(int id)
        {
            this.ViewBag.DetailsView = "DetailsViewLeaflets";
            return this.Details(id, null);
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
        public ActionResult Edit(int? id)
        {
            MedicineViewModel viewModel = new MedicineViewModel();

            if (id != null)
            {
                var medicine = this.db.Medicines.First(m => m.Id == id);
                viewModel = this.GetViewModelFromModel(medicine);
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(MedicineViewModel formModel)
        {
            // if a medicine exists with the same name, a model state error must be placed
            var existingMedicine = this.db.Medicines.FirstOrDefault(m => m.Name == formModel.Name);
            if (existingMedicine != null && existingMedicine.Id != formModel.Id)
                this.ModelState.AddModelError<MedicineViewModel>(
                    model => model.Name, "Já existe um medicamento cadastrado com o mesmo nome");

            // add validation error when Laboratory Id is invalid
            Laboratory laboratory = null;
            if (formModel.LaboratoryId != null)
            {
                laboratory = this.db.Laboratories.FirstOrDefault(lab => lab.Id == formModel.LaboratoryId && lab.DoctorId == this.Doctor.Id);
                if (laboratory == null)
                    this.ModelState.AddModelError<MedicineViewModel>((model) => model.LaboratoryId, "O laboratório informado é inválido");
            }

            if (this.ModelState.IsValid)
            {
                Medicine medicine = null;

                if (formModel.Id.HasValue)
                {
                    medicine = this.db.Medicines.FirstOrDefault(m => m.Id == formModel.Id);
                    if (medicine == null)
                        return this.ObjectNotFound();
                }
                else
                {
                    medicine = new Medicine
                        {
                            PracticeId = this.DbUser.PracticeId,
                            DoctorId = this.Doctor.Id,
                            CreatedOn = DateTime.UtcNow
                        };
                    this.db.Medicines.AddObject(medicine);
                }

                medicine.Name = formModel.Name;
                medicine.Observations = formModel.Observations;
                medicine.Usage = (short)formModel.Usage;

                if (formModel.LaboratoryId != null)
                    medicine.Laboratory = laboratory;

                // Active ingredients
                {
                    // Verify whether any existing active ingredient should be REMOVED.
                    var activeIngredientsDeathQueue = new Queue<MedicineActiveIngredient>();
                    foreach (var existingActiveIngredient in medicine.ActiveIngredients)
                        if (formModel.ActiveIngredients.All(a => a.Id != existingActiveIngredient.Id))
                            activeIngredientsDeathQueue.Enqueue(existingActiveIngredient);
                    while (activeIngredientsDeathQueue.Any())
                        this.db.ActiveIngredients.DeleteObject(activeIngredientsDeathQueue.Dequeue());

                    // Verify whether any new active ingredient should be UPDATED or ADDED
                    foreach (var activeIngredientViewModel in formModel.ActiveIngredients)
                    {
                        var existingActiveIngredient = medicine.ActiveIngredients.SingleOrDefault(l => l.Id == activeIngredientViewModel.Id);
                        if (existingActiveIngredient == null)
                        {
                            // ADD when not existing
                            medicine.ActiveIngredients.Add(
                                new MedicineActiveIngredient()
                                    {
                                        Name = activeIngredientViewModel.Name
                                    });
                        }
                        else
                        {
                            // UPDATE when existing
                            existingActiveIngredient.Name = activeIngredientViewModel.Name;
                        }
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
                                        PracticeId = this.DbPractice.Id,
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
            var medicine = this.db.Medicines.FirstOrDefault(m => m.Id == id);
            if (medicine == null)
                return this.ObjectNotFoundJson();

            if (medicine.ReceiptMedicines.Any())
                return this.Json(new { success = false, text = "Existe uma prescrição para o medicamento informado, portanto não pode ser removido" }, JsonRequestBehavior.AllowGet);

            try
            {
                // delete active ingredients manually
                var activeIngredients = medicine.ActiveIngredients.ToList();
                while (activeIngredients.Any())
                {
                    var activeIngredient = activeIngredients.First();
                    this.db.ActiveIngredients.DeleteObject(activeIngredient);
                    activeIngredients.Remove(activeIngredient);
                }

                // delete leaflets manually
                var leaflets = medicine.Leaflets.ToList();
                while (leaflets.Any())
                {
                    var leaflet = leaflets.First();
                    this.db.Leaflets.DeleteObject(leaflet);
                    leaflets.Remove(leaflet);
                }

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

        [HttpGet]
        public ActionResult AnvisaImport()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AnvisaImport(AnvisaImportViewModel formModel)
        {
            // validating the existence of the sys medicine
            var sysMedicine = this.db.SYS_Medicine.FirstOrDefault(sm => sm.Id == formModel.AnvisaId);
            if (sysMedicine == null && formModel.AnvisaId.HasValue) // I verify formModel.AnvisaId.HasValue here to prevent double errors
                this.ModelState.AddModelError<AnvisaImportViewModel>(model => model.AnvisaId, "O medicamento informado não foi encontrado");

            // validating the name is unique
            if (this.db.Medicines.Any(m => m.DoctorId == this.Doctor.Id && m.Name == sysMedicine.Name))
                this.ModelState.AddModelError<AnvisaImportViewModel>(
                    model => model.AnvisaId, "Já existe um medicamento com o mesmo nome do medicamento informado");

            if (this.ModelState.IsValid)
            {
                Debug.Assert(sysMedicine != null, "sysMedicine != null");
                var medicine = new Medicine()
                    {
                        Name = sysMedicine.Name,
                        PracticeId = this.DbPractice.Id,
                        DoctorId = this.Doctor.Id,
                        CreatedOn = DateTime.UtcNow
                    };

                // verify the need to create a new laboratory
                var laboratory = this.db.Laboratories.FirstOrDefault(l => l.DoctorId == this.Doctor.Id && l.Name == sysMedicine.Laboratory.Name) ??
                                 new Laboratory()
                                     {
                                         Name = sysMedicine.Laboratory.Name,
                                         PracticeId = this.DbPractice.Id,
                                         DoctorId = this.Doctor.Id,
                                         CreatedOn = DateTime.UtcNow
                                     };
                medicine.Laboratory = laboratory;

                // verify the need to create new active ingredients
                foreach (var ai in sysMedicine.ActiveIngredients)
                {
                    medicine.ActiveIngredients.Add(new MedicineActiveIngredient()
                    {
                        Name = ai.Name,
                        PracticeId = this.DbPractice.Id,
                    });
                }

                // create the leaflets
                foreach (var l in sysMedicine.Leaflets)
                    medicine.Leaflets.Add(
                        new Leaflet()
                            {
                                Description = l.Description,
                                Url = l.Url,
                                PracticeId = this.DbPractice.Id
                            });

                this.db.Medicines.AddObject(medicine);

                this.db.SaveChanges();

                return this.RedirectToAction("Details", new { id = medicine.Id });
            }

            return this.View(formModel);
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
                query = from medicine in query
                        where medicine.Name.Contains(searchModel.Term)
                            || medicine.Laboratory.Name.Contains(searchModel.Term)
                            || medicine.ActiveIngredients.Any(ai => ai.Name.Contains(searchModel.Term))
                        select medicine;

            // 1-based page index
            var pageIndex = searchModel.Page;
            var pageSize = Constants.GRID_PAGE_SIZE;

            model.Count = query.Count();
            model.Objects = (from m in query
                             select new MedicineViewModel()
                             {
                                 Id = m.Id,
                                 Name = m.Name,
                                 LaboratoryName = m.Laboratory.Name,
                                 LaboratoryId = m.LaboratoryId
                             }).OrderBy(p => p.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return View(model);
        }
    }
}
