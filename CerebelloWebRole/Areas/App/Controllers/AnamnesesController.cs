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
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class AnamnesesController : DoctorController
    {
        private static AnamneseViewModel GetViewModel(Anamnese anamnese)
        {
            return new AnamneseViewModel()
            {
                Id = anamnese.Id,
                PatientId = anamnese.PatientId,
                Text = anamnese.Text,
                Symptoms = (from s in anamnese.Symptoms
                            select new SymptomViewModel
                            {
                                Text = s.Cid10Name,
                                Cid10Code = s.Cid10Code

                            }).ToList()
            };
        }

        [AccessDbObject(typeof(Anamnese), "id")]
        public ActionResult Details(int id)
        {
            var anamnese = this.db.Anamnese.First(a => a.Id == id);
            return this.View(GetViewModel(anamnese));
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
        [AccessDbObject(typeof(Anamnese), "id")]
        public ActionResult Edit(int? id, int? patientId)
        {
            AnamneseViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel((from a in db.Anamnese where a.Id == id select a).First());
            else
                viewModel = new AnamneseViewModel()
                {
                    Id = null,
                    PatientId = patientId
                };

            return View("Edit", viewModel);
        }

        /// <summary>
        /// Requirements:
        ///    - The list of diagnoses passed in must be sinchronized with the server
        /// </summary>
        /// <param name="formModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AccessDbObject(typeof(Anamnese), "id")]
        public ActionResult Edit(AnamneseViewModel formModel)
        {
            Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
            if (this.ModelState.IsValid)
            {
                Anamnese anamnese = null;
                if (formModel.Id == null)
                {

                    anamnese = new Anamnese()
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value
                    };
                    this.db.Anamnese.AddObject(anamnese);
                }
                else
                    anamnese = this.db.Anamnese.First(a => a.Id == formModel.Id);

                anamnese.Text = formModel.Text;

                #region Update Symptomsymptoms
                // step 1: add new
                foreach (var symptom in formModel.Symptoms.Where(symptom => anamnese.Symptoms.All(ans => ans.Cid10Code != symptom.Cid10Code)))
                {
                    anamnese.Symptoms.Add(new Symptom()
                        {
                            Cid10Code = symptom.Cid10Code,
                            Cid10Name = symptom.Text
                        });
                }

                var harakiriQueue = new Queue<Symptom>();

                // step 2: remove deleted
                foreach (var symptom in anamnese.Symptoms.Where(symptom => formModel.Symptoms.All(ans => ans.Cid10Code != symptom.Cid10Code)))
                {
                    harakiriQueue.Enqueue(symptom);
                }

                while (harakiriQueue.Count > 0)
                    this.db.Symptoms.DeleteObject(harakiriQueue.Dequeue());
                #endregion

                db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return View("details", GetViewModel(anamnese));
            }

            return View("edit", formModel);
        }

        /// <summary>
        /// Will retrieve an editor for diagnosis. This is useful for the collection editor to request a new editor for 
        /// a newly create diagnosis at client-side
        /// </summary>
        /// <param name="formModel"></param>
        /// <returns></returns>
        public ActionResult SymptomEditor(SymptomViewModel formModel)
        {
            return View(formModel);
        }

        /// <summary>
        /// Lookup of diagnoses
        /// </summary>
        /// <remarks>
        /// Requirements:
        ///     1   Should return a AutocompleteJsonResult serialized in Json containing a the Count of CID-10 entries found
        ///         and the list of entries themselves. Each entry has a Value and an Id, the Value is the text, the Id
        ///         is the Cid10 category or sub-category of the condition
        /// </remarks>
        [HttpGet]
        public JsonResult AutocompleteDiagnoses(string term, int pageSize, int pageIndex)
        {
            IQueryable<SYS_Cid10> baseQuery = this.db.SYS_Cid10;
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(c => c.Name.Contains(term));

            var query = from c in baseQuery
                        orderby c.Name
                        select new
                        {
                            Cid10Code = c.Id,
                            Cid10Name = c.Name
                        };

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()),
                Count = query.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
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
        [AccessDbObject(typeof(Anamnese), "id")]
        public JsonResult Delete(int id)
        {
            try
            {
                var anamnese = this.db.Anamnese.First(m => m.Id == id);

                // get rid of associations
                while (anamnese.Symptoms.Count > 0)
                    this.db.Symptoms.DeleteObject(anamnese.Symptoms.ElementAt(0));

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
