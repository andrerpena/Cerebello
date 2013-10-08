using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Controller of anamneses.
    /// </summary>
    public class PastMedicalHistoriesController : DoctorController
    {
        public static PastMedicalHistoryViewModel GetViewModel(PastMedicalHistory model)
        {
            return Mapper.Map<PastMedicalHistoryViewModel>(model);
        }

        public ActionResult Details(int id)
        {
            var model = this.db.PastMedicalHistories.First(a => a.Id == id);
            var viewModel = Mapper.Map<PastMedicalHistoryViewModel>(model);
            return this.View(viewModel);
        }

        [HttpGet]
        public ActionResult Create(int patientId, int? y, int? m, int? d)
        {
            return this.Edit(null, patientId, y, m, d);
        }

        [HttpPost]
        public ActionResult Create(PastMedicalHistoryViewModel[] pastMedicalHistories)
        {
            return this.Edit(pastMedicalHistories);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId, int? y, int? m, int? d)
        {
            PastMedicalHistoryViewModel viewModel = null;

            if (id != null)
                viewModel = Mapper.Map<PastMedicalHistoryViewModel>(this.db.PastMedicalHistories.First(_m => _m.Id == id));
            else
            {
                viewModel = new PastMedicalHistoryViewModel
                {
                    Id = null,
                    PatientId = patientId,
                    MedicalRecordDate = DateTimeHelper.CreateDate(y, m, d) ?? this.GetPracticeLocalNow(),
                };
            }

            return this.View("Edit", viewModel);
        }

        /// <summary>
        /// Requirements:
        ///    - The list of diagnoses passed in must be synchronized with the server
        /// </summary>
        /// <param name="pastMedicalHistories">View model with data to edit/create an anamnese.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(PastMedicalHistoryViewModel[] pastMedicalHistories)
        {
            var viewModel = pastMedicalHistories.Single();

            Debug.Assert(viewModel.PatientId != null, "formModel.PatientId != null");
            if (this.ModelState.IsValid)
            {
                PastMedicalHistory model;
                if (viewModel.Id == null)
                {
                    model = new PastMedicalHistory
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = viewModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };
                    this.db.PastMedicalHistories.AddObject(model);
                }
                else
                    model = this.db.PastMedicalHistories.First(a => a.Id == viewModel.Id);

                Mapper.Map(viewModel, model);

                this.db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return this.View("Details", viewModel);
            }

            return this.View("Edit", viewModel);
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


            var queryResult = (from c in baseQuery.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                               select new CidAutocompleteGridModel
                                   {
                                       Cid10Code = c.Cat ?? c.SubCat,
                                       Cid10Name = c.Name
                                   }).ToList();

            var result = new AutocompleteJsonResult()
            {
                Rows = new ArrayList(queryResult),
                Count = baseQuery.Count()
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
        public JsonResult Delete(int id)
        {
            try
            {
                var anamnese = this.db.PastMedicalHistories.First(m => m.Id == id);
                this.db.PastMedicalHistories.DeleteObject(anamnese);
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
