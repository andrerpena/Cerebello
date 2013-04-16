using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ExamsController : DoctorController
    {
        public static ExaminationRequestViewModel GetViewModel(ExaminationRequest examRequest)
        {
            return new ExaminationRequestViewModel()
            {
                Id = examRequest.Id,
                PatientId = examRequest.PatientId,
                Notes = examRequest.Text,
                MedicalProcedureName = examRequest.MedicalProcedureName,
                MedicalProcedureCode = examRequest.MedicalProcedureCode,
                RequestDate = examRequest.RequestDate,
            };
        }

        public ActionResult Details(int id)
        {
            var examRequest = this.db.ExaminationRequests.First(a => a.Id == id);
            return this.View(GetViewModel(examRequest));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(ExaminationRequestViewModel[] examRequest)
        {
            return this.Edit(examRequest);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            ExaminationRequestViewModel viewModel = null;

            // Todo: security issue... must check current user practice againt the practice of the edited objects.

            if (id != null)
            {
                var modelObj = this.db.ExaminationRequests.FirstOrDefault(r => r.Id == id);

                // todo: if modelObj is null, we must tell the user that this object does not exist.

                viewModel = GetViewModel(modelObj);
            }
            else
                viewModel = new ExaminationRequestViewModel()
                {
                    Id = null,
                    PatientId = patientId,
                    RequestDate = this.GetPracticeLocalNow(),
                };

            return this.View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ExaminationRequestViewModel[] examRequest)
        {
            var formModel = examRequest[0];

            ExaminationRequest dbObject;

            if (formModel.Id == null)
            {
                Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
                dbObject = new ExaminationRequest
                {
                    CreatedOn = this.GetUtcNow(),
                    PatientId = formModel.PatientId.Value,
                    PracticeId = this.DbUser.PracticeId,
                };

                this.db.ExaminationRequests.AddObject(dbObject);
            }
            else
            {
                dbObject = this.db.ExaminationRequests.FirstOrDefault(r => r.Id == formModel.Id);

                // If modelObj is null, we must tell the user that this object does not exist.
                if (dbObject == null)
                    return View("NotFound", formModel);

                // Security issue... must check current user practice against the practice of the edited objects.
                if (this.DbUser.Practice.Id != dbObject.Patient.Doctor.Users.FirstOrDefault().PracticeId)
                    return View("NotFound", formModel);
            }

            if (this.ModelState.IsValid)
            {
                dbObject.Text = formModel.Notes;
                dbObject.MedicalProcedureCode = formModel.MedicalProcedureId.HasValue
                    ? this.db.SYS_MedicalProcedure.Where(mp => mp.Id == formModel.MedicalProcedureId).Select(mp => mp.Code).FirstOrDefault()
                    : null;

                dbObject.MedicalProcedureName = formModel.MedicalProcedureName;
                dbObject.RequestDate = formModel.RequestDate.Value;

                db.SaveChanges();

                return this.View("Details", GetViewModel(dbObject));
            }

            return this.View("Edit", formModel);
        }

        /// <summary>
        /// 
        /// Requisitos:
        ///     
        ///     1 - The given exam-request should stop existing after this action call. In case of success, it should return a JsonDeleteMessage
        ///         with success = true
        ///     
        ///     2 - In case the given exam-request doesn't exist,
        ///         it should return a JsonDeleteMessage with success = false and the text property
        ///         informing that the object does not exist.
        ///     
        ///     3 - In case the given exam-request doesn't belong to the current user practice,
        ///         it should return a JsonDeleteMessage with success = false and the text property
        ///         informing that the object does not exist.
        /// </summary>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var practiceId = this.DbUser.PracticeId;

                var modelObj = this.db.ExaminationRequests
                    .Where(m => m.Id == id)
                    .FirstOrDefault(m => m.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId);

                // If exam does not exist, return message telling this.
                if (modelObj == null)
                {
                    return this.Json(
                        new JsonDeleteMessage { success = false, text = "A requisição de exame não existe." },
                        JsonRequestBehavior.AllowGet);
                }

                this.db.ExaminationRequests.DeleteObject(modelObj);
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
