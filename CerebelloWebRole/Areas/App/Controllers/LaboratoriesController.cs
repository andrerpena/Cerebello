using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls.Autocomplete.Data;
using JetBrains.Annotations;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class LaboratoriesController : DoctorController
    {
        public MedicineLaboratoryViewModel GetViewModel(Laboratory laboratory, int? page = null)
        {
            if (laboratory == null)
                return new MedicineLaboratoryViewModel();

            if (!page.HasValue)
                page = 1;
            var pageSize = Constants.GRID_PAGE_SIZE;

            var medicinesQuery = from medicine in laboratory.Medicines
                                 orderby medicine.Name
                                 select new MedicineViewModel()
                                 {
                                     Id = medicine.Id,
                                     Name = medicine.Name,
                                     Usage = medicine.Usage
                                 };

            return new MedicineLaboratoryViewModel()
            {
                Id = laboratory.Id,
                Name = laboratory.Name,
                Observations = laboratory.Observations,
                Medicines = new SearchViewModel<MedicineViewModel>()
                {
                    Objects = medicinesQuery.Skip((page.Value - 1) * pageSize).Take(pageSize).ToList(),
                    Count = medicinesQuery.Count()
                },
            };
        }

        public ActionResult Index()
        {
            var viewModel = new LaboratoriesIndexViewModel
                {
                    LastRegisteredLaboratories =
                        (from laboratory in
                             this.db.Laboratories.Where(l => l.DoctorId == this.Doctor.Id).OrderByDescending(l => l.CreatedOn).Take(5).ToList()
                         select this.GetViewModel(laboratory)).ToList(),
                    TotalLaboratoriesCount = this.db.Laboratories.Count()
                };

            return this.View(viewModel);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var laboratory = this.db.Laboratories.FirstOrDefault(l => l.Id == id);
            if (laboratory == null)
                return this.ObjectNotFound();

            var viewModel = this.GetViewModel(laboratory);

            return this.View(viewModel);
        }


            [HttpGet]
        public JsonResult Delete(int id)
        {
            var laboratory = this.db.Laboratories.FirstOrDefault(l => l.Id == id);
            if (laboratory == null)
                return this.ObjectNotFoundJson();

            if (laboratory.Medicines.Any(m => m.ReceiptMedicines.Any()))
                return this.Json(new { success = false, text = "O laboratório informado está vinculado a um medicamento com prescrições, portanto não pode ser removido" }, JsonRequestBehavior.AllowGet);

            try
            {
                // delete medicines manually
                var medicines = laboratory.Medicines.ToList();
                while (medicines.Any())
                {
                    var medicine = medicines.First();

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
                    medicines.Remove(medicine);
                }

                this.db.Laboratories.DeleteObject(laboratory);
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CreateModal()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult CreateModal([NotNull] MedicineLaboratoryViewModel formModel)
        {
            if (formModel == null) throw new ArgumentNullException("formModel");
            return this.Edit(formModel);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult Create(MedicineLaboratoryViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            MedicineLaboratoryViewModel viewModel = null;
            if (id.HasValue)
            {
                var laboratory = this.db.Laboratories.FirstOrDefault(l => l.Id == id);
                if (laboratory == null)
                    return this.ObjectNotFound();

                viewModel = this.GetViewModel(laboratory);
            }

            return View("edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit([NotNull] MedicineLaboratoryViewModel formModel)
        {
            if (formModel == null) throw new ArgumentNullException("formModel");

            // if a laboratory exists with the same name, a model state error must be placed
            var existingLaboratory = this.db.Laboratories.FirstOrDefault(l => l.Name == formModel.Name);
            if (existingLaboratory != null && existingLaboratory.Id != formModel.Id)
                this.ModelState.AddModelError<MedicineLaboratoryViewModel>(model => model.Name, "Já existe um laboratório cadastrado com o mesmo nome");

            if (this.ModelState.IsValid)
            {
                Laboratory laboratory = null;
                if (formModel.Id.HasValue)
                {
                    laboratory = this.db.Laboratories.FirstOrDefault(l => l.Id == formModel.Id);
                    if (laboratory == null)
                        return this.ObjectNotFound();
                }
                else
                {
                    laboratory = new Laboratory()
                        {
                            PracticeId = this.DbUser.PracticeId,
                            DoctorId = this.Doctor.Id,
                            CreatedOn = DateTime.UtcNow
                        };
                    this.db.Laboratories.AddObject(laboratory);
                }

                laboratory.Name = formModel.Name;
                laboratory.Observations = formModel.Observations;

                this.db.SaveChanges();

                // depending on whether or not this is an Ajax request,
                // this should return an AutocompleteNewJsonResult or the view
                if (this.Request.IsAjaxRequest())
                    return this.Json(
                        new AutocompleteNewJsonResult()
                            {
                                Id = laboratory.Id,
                                Value = laboratory.Name
                            });

                // The View here will DEPEND on the caller.
                // If it's EditModal, it's gonna be EditModal. Otherwise, Edit
                return this.RedirectToAction("details", new { id = laboratory.Id });
            }

            return this.View("edit", formModel);
        }

        public ActionResult Search(SearchModel searchModel)
        {
            var model = new SearchViewModel<MedicineLaboratoryViewModel>();

            var query = from laboratory in db.Laboratories
                        where laboratory.DoctorId == this.Doctor.Id
                        select laboratory;

            if (!string.IsNullOrEmpty(searchModel.Term))
                query = from laboratory in query where laboratory.Name.Contains(searchModel.Term) select laboratory;

            // 1-based page index
            var pageIndex = searchModel.Page;
            var pageSize = Constants.GRID_PAGE_SIZE;

            model.Count = query.Count();
            model.Objects = (from m in query
                             select new MedicineLaboratoryViewModel()
                             {
                                 Id = m.Id,
                                 Name = m.Name,
                             }).OrderBy(p => p.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return View(model);
        }


    }

    
}