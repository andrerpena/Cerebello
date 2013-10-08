using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    // todo: plural of diagnosis is diagnoses
    public class DiagnosisController : DoctorController
    {
        public static DiagnosisViewModel GetViewModel(Diagnosis model)
        {
            return Mapper.Map<DiagnosisViewModel>(model);
        }

        public ActionResult Details(int id)
        {
            var model = this.db.Diagnoses.First(a => a.Id == id);
            var viewModel = Mapper.Map<DiagnosisViewModel>(model);
            return this.View(viewModel);
        }

        [HttpGet]
        public ActionResult Create(int patientId, int? y, int? m, int? d)
        {
            return this.Edit(null, patientId, y, m, d);
        }

        [HttpPost]
        public ActionResult Create(DiagnosisViewModel[] diagnoses)
        {
            return this.Edit(diagnoses);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId, int? y, int? m, int? d)
        {
            DiagnosisViewModel viewModel = null;

            if (id != null)
                viewModel = Mapper.Map<DiagnosisViewModel>(db.Diagnoses.First(_m => _m.Id == id));
            else
                viewModel = new DiagnosisViewModel()
                {
                    Id = null,
                    PatientId = patientId,
                    MedicalRecordDate = DateTimeHelper.CreateDate(y, m, d) ?? this.GetPracticeLocalNow(),
                };

            return this.View("Edit", viewModel);
        }

        /// <summary>
        /// Requirements:
        ///    - If both the notes and the Cid10Code are null or empty, an error must be added to ModelState
        ///    - Must properly create a diagnosis for the given patient with the given information
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(DiagnosisViewModel[] diagnoses)
        {
            var viewModel = diagnoses.Single();

            if (this.ModelState.IsValid)
            {
                Diagnosis model;
                if (viewModel.Id == null)
                {
                    Debug.Assert(viewModel.PatientId != null, "formModel.PatientId != null");
                    model = new Diagnosis()
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = viewModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };
                    this.db.Diagnoses.AddObject(model);
                }
                else
                    model = this.db.Diagnoses.First(a => a.Id == viewModel.Id);

                Mapper.Map(viewModel, model);
                this.db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return this.View("Details", viewModel);
            }

            return this.View("Edit", viewModel);
        }

        /// <summary>
        /// 
        /// Requisitos:
        ///     
        ///     1 - The given diagnosis should stop existing after this action call. In case of success, it should return a JsonDeleteMessage
        ///         with success = true
        ///     
        ///     2 - In case the given diagnosis doesn't exist, it should return a JsonDeleteMessage with success = false and the text property
        ///         informing the reason of the failure
        /// 
        /// </summary>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var diagnosis = this.db.Diagnoses.First(m => m.Id == id);
                this.db.Diagnoses.DeleteObject(diagnosis);
                this.db.SaveChanges();
                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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
                baseQuery = baseQuery.Where(c => c.Name.Contains(term) || c.Cat.Contains(term) || c.SubCat.Contains(term));

            var query = from c in baseQuery
                        orderby c.Name
                        select new // CidAutocompleteGridModel
                        {
                            Cid10Code = c.Cat ?? c.SubCat,
                            Cid10Name = c.Name
                        };

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()),
                Count = query.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}