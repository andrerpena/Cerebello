using System;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ExamResultsController : DoctorController
    {
        public static ExaminationResultViewModel GetViewModel(ExaminationResult examResult)
        {
            return new ExaminationResultViewModel
                       {
                           Id = examResult.Id,
                           PatientId = examResult.PatientId,
                           Text = examResult.Text,
                           MedicalProcedureCode = examResult.MedicalProcedureCode,
                           MedicalProcedureName = examResult.MedicalProcedureName,
                       };
        }

        public ActionResult Details(int id)
        {
            var practiceId = this.DbUser.PracticeId;

            var examResult = this.db.ExaminationResults
                .Where(r => r.Id == id)
                .First(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId);

            return this.View(GetViewModel(examResult));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(ExaminationResultViewModel[] examResult)
        {
            return this.Edit(examResult);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            ExaminationResultViewModel viewModel;

            if (id != null)
            {
                var practiceId = this.DbUser.PracticeId;

                var modelObj = this.db.ExaminationResults
                    .Where(r => r.Id == id)
                    .FirstOrDefault(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId);

                // todo: if modelObj is null, we must tell the user that this object does not exist.

                viewModel = GetViewModel(modelObj);
            }
            else
            {
                viewModel = new ExaminationResultViewModel
                                {
                                    Id = null,
                                    PatientId = patientId
                                };
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ExaminationResultViewModel[] examResult)
        {
            ExaminationResult dbObject;

            var formModel = examResult[0];

            if (formModel.Id == null)
            {
                dbObject = new ExaminationResult
                {
                    CreatedOn = this.GetUtcNow(),
                    PatientId = formModel.PatientId.Value,
                    PracticeId = this.DbUser.PracticeId,
                };

                this.db.ExaminationResults.AddObject(dbObject);
            }
            else
            {
                var practiceId = this.DbUser.PracticeId;

                dbObject = this.db.ExaminationResults
                    .Where(r => r.Id == formModel.Id)
                    .FirstOrDefault(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId);

                // If modelObj is null, we must tell the user that this object does not exist.
                if (dbObject == null)
                    return View("NotFound", formModel);

                // Security issue... must check current user practice against the practice of the edited objects.
                var currentUser = this.DbUser;
                if (currentUser.PracticeId != dbObject.Patient.Doctor.Users.First().PracticeId)
                    return View("NotFound", formModel);
            }

            if (this.ModelState.IsValid)
            {
                dbObject.Text = formModel.Text;
                dbObject.MedicalProcedureName = formModel.MedicalProcedureName;
                dbObject.MedicalProcedureCode = formModel.MedicalProcedureId.HasValue
                    ? this.db.SYS_MedicalProcedure.Where(mp => mp.Id == formModel.MedicalProcedureId).Select(mp => mp.Code).FirstOrDefault()
                    : null;

                db.SaveChanges();

                return View("Details", GetViewModel(dbObject));
            }

            return View("Edit", formModel);
        }

        /// <summary>
        /// 
        /// Requisitos:
        ///     
        ///     1 - The given exam-result should stop existing after this action call.
        ///         In case of success, it should return a JsonDeleteMessage
        ///         with success = true
        ///     
        ///     2 - In case the given exam-result doesn't exist,
        ///         it should return a JsonDeleteMessage with success = false and the text property
        ///         informing that the object does not exist.
        ///     
        ///     3 - In case the given exam-result doesn't belong to the current user practice,
        ///         it should return a JsonDeleteMessage with success = false and the text property
        ///         informing that the object does not exist.
        /// </summary>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var practiceId = this.DbUser.PracticeId;

                var modelObj = this.db.ExaminationResults
                    .Where(r => r.Id == id)
                    .FirstOrDefault(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId);

                // If exam does not exist, return message telling this.
                if (modelObj == null)
                {
                    return this.Json(
                        new JsonDeleteMessage { success = false, text = "O resultado de exame não existe." },
                        JsonRequestBehavior.AllowGet);
                }

                this.db.ExaminationResults.DeleteObject(modelObj);
                this.db.SaveChanges();
                return this.Json(
                    new JsonDeleteMessage { success = true },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(
                    new JsonDeleteMessage { success = false, text = ex.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}
