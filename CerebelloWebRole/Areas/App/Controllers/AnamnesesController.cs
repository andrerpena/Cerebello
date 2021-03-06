﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Controller of anamneses.
    /// </summary>
    public class AnamnesesController : DoctorController
    {
        /// <summary>
        /// Constructs a view-model for an anamnesis, given the DB object representing it.
        /// </summary>
        /// <param name="anamnese">The DB object representing the anamnesis.</param>
        /// <returns>The view-model that contains data about the anamnesis.</returns>
        public static AnamneseViewModel GetViewModel(Anamnese anamnese, Func<DateTime, DateTime> toLocal)
        {
            return new AnamneseViewModel
            {
                Id = anamnese.Id,
                PatientId = anamnese.PatientId,
                Allergies = anamnese.Allergies,
                ChiefComplaint = anamnese.ChiefComplaint,
                Conclusion = anamnese.Conclusion,
                FamilyDeseases = anamnese.FamilyDiseases,
                HistoryOfThePresentIllness = anamnese.HistoryOfThePresentIllness,
                PastMedicalHistory = anamnese.PastMedicalHistory,
                RegularAndAcuteMedications = anamnese.RegularAndAcuteMedications,
                ReviewOfSystems = anamnese.ReviewOfSystems,
                SexualHistory = anamnese.SexualHistory,
                SocialHistory = anamnese.SocialDiseases,
                MedicalRecordDate = toLocal(anamnese.MedicalRecordDate),
            };
        }

        public ActionResult Details(int id)
        {
            var anamnese = this.db.Anamnese.First(a => a.Id == id);
            return this.View(GetViewModel(anamnese, this.GetToLocalDateTimeConverter()));
        }

        [HttpGet]
        public ActionResult Create(int patientId, int? y, int? m, int? d)
        {
            return this.Edit(null, patientId, y, m, d);
        }

        [HttpPost]
        public ActionResult Create(AnamneseViewModel[] anamneses)
        {
            return this.Edit(anamneses);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId, int? y, int? m, int? d)
        {
            AnamneseViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel(
                    (from a in this.db.Anamnese where a.Id == id select a).First(),
                    this.GetToLocalDateTimeConverter());
            else
            {
                viewModel = new AnamneseViewModel
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
        /// <param name="anamneses">View model with data to edit/create an anamnese.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(AnamneseViewModel[] anamneses)
        {
            var formModel = anamneses.Single();

            Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
            if (this.ModelState.IsValid)
            {
                Anamnese anamnese = null;
                if (formModel.Id == null)
                {

                    anamnese = new Anamnese
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };
                    this.db.Anamnese.AddObject(anamnese);
                }
                else
                    anamnese = this.db.Anamnese.First(a => a.Id == formModel.Id);

                anamnese.Patient.IsBackedUp = false;
                anamnese.Allergies = formModel.Allergies;
                anamnese.ChiefComplaint = formModel.ChiefComplaint;
                anamnese.Conclusion = formModel.Conclusion;
                anamnese.FamilyDiseases = formModel.FamilyDeseases;
                anamnese.HistoryOfThePresentIllness = formModel.HistoryOfThePresentIllness;
                anamnese.PastMedicalHistory = formModel.PastMedicalHistory;
                anamnese.RegularAndAcuteMedications = formModel.RegularAndAcuteMedications;
                anamnese.ReviewOfSystems = formModel.ReviewOfSystems;
                anamnese.SexualHistory = formModel.SexualHistory;
                anamnese.SocialDiseases = formModel.SocialHistory;
                anamnese.MedicalRecordDate = this.ConvertToUtcDateTime(formModel.MedicalRecordDate.Value);

                this.db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return this.View("Details", GetViewModel(anamnese, this.GetToLocalDateTimeConverter()));
            }

            return this.View("Edit", formModel);
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
                var anamnese = this.db.Anamnese.First(m => m.Id == id);

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
