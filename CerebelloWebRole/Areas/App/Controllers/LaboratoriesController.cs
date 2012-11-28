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

        public ActionResult Index(int? page)
        {
            return this.View();
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

            var laboratory = this.db.Laboratories.FirstOrDefault(l => l.Name == formModel.Name);

            if (laboratory != null && laboratory.Id != formModel.Id)
                this.ModelState.AddModelError<MedicineLaboratoryViewModel>(model => model.Name, "Já existe um laboratório cadastrado com o mesmo nome");

            if (this.ModelState.IsValid)
            {
                laboratory = new Laboratory()
                {
                    Name = formModel.Name,
                    Observations = formModel.Observations,
                    Doctor = this.Doctor,
                    PracticeId = this.Practice.Id
                };

                this.db.Laboratories.AddObject(laboratory);
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


    }
}