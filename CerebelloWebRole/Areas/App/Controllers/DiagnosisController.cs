using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class DiagnosisController : DoctorController
    {
        public static DiagnosisViewModel GetViewModel(Diagnosis diagnosis)
        {
            return new DiagnosisViewModel()
            {
                Id = diagnosis.Id,
                PatientId = diagnosis.PatientId,
                Text = diagnosis.Observations,
                Cid10Code = diagnosis.Cid10Code,
                Cid10Name = diagnosis.Cid10Name,
                MedicalRecordDate = diagnosis.MedicalRecordDate,
            };
        }

        [HttpGet]
        public ActionResult Create(int patientId)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(DiagnosisViewModel[] diagnosis)
        {
            return this.Edit(diagnosis);
        }

        public ActionResult Details(int id)
        {
            var diagnosis = this.db.Diagnoses.First(a => a.Id == id);
            return this.View(GetViewModel(diagnosis));
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            DiagnosisViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel((from a in db.Diagnoses where a.Id == id select a).First());
            else
                viewModel = new DiagnosisViewModel()
                {
                    Id = null,
                    PatientId = patientId,
                    MedicalRecordDate = this.GetPracticeLocalNow(),
                };

            return View("Edit", viewModel);
        }

        /// <summary>
        /// Requirements:
        ///    - If both the notes and the Cid10Code are null or empty, an error must be added to ModelState
        ///    - Must properly create a diagnosis for the given patient with the given information
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(DiagnosisViewModel[] diagnosis)
        {
            var formModel = diagnosis[0];

            if (string.IsNullOrEmpty(formModel.Text) && string.IsNullOrEmpty(formModel.Cid10Name))
                this.ModelState.AddModelError("", "É necessário preencher um diagnóstico CID-10 ou as notas");

            if (this.ModelState.IsValid)
            {
                Diagnosis dbObject;
                if (formModel.Id == null)
                {
                    Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
                    dbObject = new Diagnosis()
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };
                    this.db.Diagnoses.AddObject(dbObject);
                }
                else
                    dbObject = this.db.Diagnoses.First(a => a.Id == formModel.Id);

                dbObject.Observations = formModel.Text;
                dbObject.Cid10Code = formModel.Cid10Code;
                dbObject.Cid10Name = formModel.Cid10Name;
                dbObject.MedicalRecordDate = formModel.MedicalRecordDate;
                db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return View("Details", GetViewModel(dbObject));
            }

            return View("Edit", formModel);
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