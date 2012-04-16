using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using Cerebello.Model;
using CommonLib.Mvc;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class AnamnesesController : DoctorController
    {
        private AnamneseViewModel GetViewModel(Anamnese anamnese)
        {
            return new AnamneseViewModel()
            {
                Id = anamnese.Id,
                PatientId = anamnese.PatientId,
                Text = anamnese.Text,
                AnamneseSymptoms = (from s in anamnese.AnamneseSymptoms
                                    select new AnamneseSymptomViewModel
                                    {
                                        Text = s.Symptom.Name
                                    }).ToList()
            };
        }

        public ActionResult Details(int id)
        {
            var anamnese = db.Anamnese.Where(a => a.Id == id).First();
            return this.View(this.GetViewModel(anamnese));
        }

        [HttpGet]
        public ActionResult Create(int patientId)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(AnamneseViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            AnamneseViewModel viewModel = null;

            if (id != null)
                viewModel = this.GetViewModel((from a in db.Anamnese where a.Id == id select a).First());
            else
                viewModel = new AnamneseViewModel()
                {
                    Id = id,
                    PatientId = patientId
                };

            return View("Edit", viewModel);
        }

        /// <summary>
        /// Requirements:
        /// 
        ///     1 - For each symptom, if it doesn't exist, it must be created
        ///     
        ///     2 - For each symptom, if it exists, it should be referenced, not created
        ///     
        ///     2 - For each symptom, if it existed but doesn't exist anymore, it must be deleted
        ///     
        /// </summary>
        /// <param name="formModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(AnamneseViewModel formModel)
        {
            Anamnese anamnese = null;

            if (this.ModelState.IsValid)
            {
                if (formModel.Id == null)
                {
                    anamnese = new Anamnese()
                    {
                        CreatedOn = DateTime.UtcNow,
                        PatientId = formModel.PatientId.Value
                    };
                    this.db.Anamnese.AddObject(anamnese);
                }
                else
                    anamnese = db.Anamnese.Where(a => a.Id == formModel.Id).FirstOrDefault();

                anamnese.Text = formModel.Text;

                #region Update Symptoms
                // step 1: add new
                foreach (var symptomViewModel in formModel.AnamneseSymptoms)
                {
                    if (!anamnese.AnamneseSymptoms.Any(ans => ans.Symptom.Name == symptomViewModel.Text))
                    {
                        // the symptom has beed added, now we have to verify whether this symptom exists already
                        var possiblyExistingSymptom = this.db.Symptoms.FirstOrDefault(s => s.Name == symptomViewModel.Text);
                        if (possiblyExistingSymptom != null)
                            anamnese.AnamneseSymptoms.Add(new AnamneseSymptom() { 
                                Symptom = possiblyExistingSymptom
                            });
                                
                        else
                            anamnese.AnamneseSymptoms.Add(new AnamneseSymptom()
                            {
                                Symptom = new Symptom()
                                {
                                    Doctor = this.Doctor,
                                    Name = symptomViewModel.Text
                                }
                            });
                    }
                }

                Queue<AnamneseSymptom> harakiriQueue = new Queue<AnamneseSymptom>();

                // step 2: remove deleted
                foreach (var anamneseSymptomModel in anamnese.AnamneseSymptoms)
                {
                    if (!formModel.AnamneseSymptoms.Any(ans => ans.Text == anamneseSymptomModel.Symptom.Name))
                        harakiriQueue.Enqueue(anamneseSymptomModel);
                }

                while (harakiriQueue.Count > 0)
                    anamnese.AnamneseSymptoms.Remove(harakiriQueue.Dequeue());
                #endregion 

                db.SaveChanges();

                return View("details", this.GetViewModel(anamnese));
            }
            return View("edit", formModel);
        }

        public ActionResult AnamneseSymptomEditor(AnamneseSymptomViewModel formModel)
        {
            return View(formModel);
        }

        [HttpGet]
        public JsonResult SearchSymptom(string term)
        {
            var medicines = (from s in this.db.Symptoms
                             where s.Name.Contains(term)
                             orderby s.Name
                             select new
                             {
                                 id = s.Id,
                                 value = s.Name
                             }).Take(5).ToList();

            return this.Json(medicines, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// Requisitos:
        ///     
        ///     1 - The given anamnese should stop existing after this action call. In case of success, it should return a JsonDeleteMessage
        ///         with success = true
        ///     
        ///     2 - In case the given anamnese doesn't exist, it should return a JsonDeleteMessage with success = false and the text property
        ///         informing the reason of the failure
        /// 
        /// </summary>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var anamnese = db.Anamnese.Where(m => m.Id == id).First();

                // get rid of associations
                while (anamnese.AnamneseSymptoms.Count > 0)
                    this.db.AnamneseSymptoms.DeleteObject(anamnese.AnamneseSymptoms.ElementAt(0));

                this.db.Anamnese.DeleteObject(anamnese);
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
