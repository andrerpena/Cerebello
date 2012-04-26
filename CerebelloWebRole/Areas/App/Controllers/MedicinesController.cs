using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Models;
using Cerebello.Model;
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
            model.Medicines = (from medicine in db.Medicines.Where(m => m.Doctor.Id == this.Doctor.Id).OrderBy(m => m.Name).Take(5).ToList()
                               select this.GetViewModelFromModel(medicine)).ToList();
            model.Count = db.Medicines.Count();
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
                var medicine = db.Medicines.Where(m => m.Id == id).First();
                viewModel = this.GetViewModelFromModel(medicine);
                ViewBag.Title = "Alterando medicamento: " + viewModel.Name;
            }
            else
                ViewBag.Title = "Novo medicamento";

            if (anvisaId != null)
            {
                var sysMedicine = db.SYS_Medicine.Where(sm => sm.Id == anvisaId).FirstOrDefault();

                viewModel.Name = sysMedicine.Name;


                // verify if there's already this lab in the user database
                var existingLab = db.Laboratories.Where(l => l.Name == sysMedicine.Laboratory.Name && l.DoctorId == this.Doctor.Id).FirstOrDefault();
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
            if (formModel.ActiveIngredients == null || formModel.ActiveIngredients.Count == 0)
                this.ModelState.AddModelError("activeIngredients", "O medicamento deve possuir pelo menos um princípio ativo");

            if (formModel.LaboratoryId == null && string.IsNullOrEmpty(formModel.LaboratoryName))
                this.ModelState.AddModelError("laboraroty", "É necessário informar o laboratório");

            //TODO: adicionar validação pra ver se os IDs do laboratórios e dos princípios ativos existem de verdade

            if (this.ModelState.IsValid)
            {
                Medicine medicine = null;

                if (formModel.Id != null)
                    medicine = db.Medicines.Where(m => m.Id == formModel.Id).First();
                else
                {
                    medicine = new Medicine();
                    this.db.Medicines.AddObject(medicine);
                }

                medicine.Name = formModel.Name;
                medicine.Usage = (short)formModel.Usage;
                medicine.Doctor = this.Doctor;

                if (formModel.LaboratoryId != null)
                    medicine.Laboratory = db.Laboratories.Where(lab => lab.Id == formModel.LaboratoryId && lab.DoctorId == this.Doctor.Id).First();
                else
                    medicine.Laboratory = new Laboratory()
                    {
                        Name = formModel.LaboratoryName,
                        Doctor = this.Doctor
                    };

                medicine.ActiveIngredients.Update(
                    formModel.ActiveIngredients,
                    (vm, m) => vm.ActiveIngredientId == m.Id,
                    (vm, m) =>
                    {
                        m.DoctorId = this.Doctor.Id;
                        m.Name = vm.ActiveIngredientName;
                    },
                    (m) => medicine.ActiveIngredients.Remove(m)
                    , EntityObjectExtensions.CollectionUpdateStrategy.Create);

                medicine.Leaflets.Update(
                    formModel.Leaflets,
                    (vm, m) => vm.Id == m.Id,
                    (vm, m) =>
                    {
                        m.Description = vm.Description;
                        m.Url = vm.Url;
                    },
                    (m) => medicine.Leaflets.Remove(m));

                db.SaveChanges();

                return Redirect(Url.Action("details", new { id = medicine.Id }));
            }

            return View("Edit", formModel);
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            var medicine = db.Medicines.Where(m => m.Id == id).First();
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
        public JsonResult SearchLaboratory(string term)
        {
            var laboratories = (from l in this.db.Laboratories
                                where l.Doctor.Id == this.Doctor.Id && l.Name.Contains(term)
                                select new
                                {
                                    id = l.Id,
                                    value = l.Name
                                }).Take(5).ToList();

            return this.Json(laboratories, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActiveIngredientEditor(MedicineActiveIngredientViewModel viewModel)
        {
            return View(viewModel);
        }

        [HttpGet]
        public JsonResult SearchMedication(string term)
        {
            var medicines = (from m in this.db.Medicines
                             where (m.Doctor.Id == this.Doctor.Id) && (m.Name.Contains(term) || m.ActiveIngredients.Any(ai => ai.Name.Contains(term)))
                             orderby m.Name
                             select new
                             {
                                 id = m.Id,
                                 value = m.Name,
                                 laboratory = m.Laboratory.Name,
                                 activeIngredients = m.ActiveIngredients.Select(ai => ai.Name)
                             }).Take(5).ToList();

            return this.Json(medicines, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SearchActiveIngredient(string term)
        {
            var activeIngredients = (from ai in this.db.ActiveIngredients
                                     where (ai.Doctor.Id == this.Doctor.Id) && (ai.Name.Contains(term))
                                     orderby ai.Name
                                     select new
                                     {
                                         id = ai.Id,
                                         value = ai.Name
                                     }).Take(5).ToList();

            return this.Json(activeIngredients, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SearchSysMedication(string term)
        {
            var medicines = (from m in this.db.SYS_Medicine
                             where (m.Name.Contains(term) || m.ActiveIngredients.Any(ai => ai.Name.Contains(term)))
                             orderby m.Name
                             select new
                             {
                                 id = m.Id,
                                 value = m.Name,
                                 laboratory = m.Laboratory.Name,
                                 activeIngredients = m.ActiveIngredients.Select(ai => ai.Name)
                             }).Take(5).ToList();

            return this.Json(medicines, JsonRequestBehavior.AllowGet);
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
    }
}
