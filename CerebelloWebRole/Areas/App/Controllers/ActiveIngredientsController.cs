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
    public class ActiveIngredientsController : DoctorController
    {
        [HttpGet]
        public ActionResult CreateModal()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult CreateModal([NotNull] MedicineActiveIngredientViewModel formModel)
        {
            if (formModel == null) throw new ArgumentNullException("formModel");
            // this is necessary to explicitly state the View name here because
            // CreateModal may redirect to here
            return this.Edit(formModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit([NotNull] MedicineActiveIngredientViewModel formModel)
        {
            if (formModel == null) throw new ArgumentNullException("formModel");

            var activeIngredient = this.db.ActiveIngredients.FirstOrDefault(l => l.Name == formModel.ActiveIngredientName);

            if (activeIngredient != null)
                this.ModelState.AddModelError<MedicineActiveIngredientViewModel>(model => model.ActiveIngredientName, "Já existe um princípio ativo cadastrado com o mesmo nome");

            if (this.ModelState.IsValid)
            {
                activeIngredient = new ActiveIngredient()
                {
                    Name = formModel.ActiveIngredientName,
                    Doctor = this.Doctor,
                    PracticeId = this.Practice.Id
                };

                this.db.ActiveIngredients.AddObject(activeIngredient);
                this.db.SaveChanges();

                // depending on whether or not this is an Ajax request,
                // this should return an AutocompleteNewJsonResult or the view
                if (this.Request.IsAjaxRequest())
                    return this.Json(
                        new AutocompleteNewJsonResult()
                        {
                            Id = activeIngredient.Id,
                            Value = activeIngredient.Name
                        });

                // The View here will DEPEND on the caller.
                // If it's EditModal, it's gonna be EditModal. Otherwise, Edit
                return this.View(
                    new MedicineActiveIngredientViewModel()
                    {
                        ActiveIngredientId = activeIngredient.Id,
                        ActiveIngredientName = activeIngredient.Name
                    });
            }

            return this.View(formModel);
        }
    }
}