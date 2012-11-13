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
        [HttpGet]
        public ActionResult CreateModal()
        {
            return this.EditModal((int?)null);
        }

        [HttpGet]
        public ActionResult EditModal(int? id)
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult EditModal([NotNull] MedicineLaboratoryViewModel formModel)
        {
            if (formModel == null) throw new ArgumentNullException("formModel");
            return this.Edit(formModel);
        }

        [HttpPost]
        public ActionResult Edit([NotNull] MedicineLaboratoryViewModel formModel)
        {
            if (formModel == null) throw new ArgumentNullException("formModel");

            var laboratory = this.db.Laboratories.FirstOrDefault(l => l.Name == formModel.Name);

            if (laboratory != null)
                this.ModelState.AddModelError<MedicineLaboratoryViewModel>(model => model.Name, "Já existe um laboratório cadastrado com o mesmo nome");

            if (this.ModelState.IsValid)
            {
                laboratory = new Laboratory()
                {
                    Name = formModel.Name,
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
                return this.View(
                    new MedicineLaboratoryViewModel()
                        {
                            Id = laboratory.Id,
                            Name = laboratory.Name
                        });
            }

            return this.View(formModel);
        }
    }
}