using System;
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
                    Id = id,
                    PatientId = patientId
                };

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ExaminationRequestViewModel[] examRequest)
        {
            var formModel = examRequest[0];

            ExaminationRequest dbObject = null;

            if (formModel.Id == null)
            {
                dbObject = new ExaminationRequest
                {
                    CreatedOn = DateTimeHelper.UtcNow,
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
                {
                    return View("NotFound", formModel);
                }

                // Security issue... must check current user practice against the practice of the edited objects.
                if (this.DbUser.Practice.Id != dbObject.Patient.Doctor.Users.FirstOrDefault().PracticeId)
                {
                    return View("NotFound", formModel);
                }
            }

            dbObject.Text = formModel.Notes;

            // Only sets the MedicalProcedureId when MedicalProcedureText is not null.
            if (formModel.MedicalProcedureId != null || !string.IsNullOrEmpty(formModel.MedicalProcedureName))
            {
                var mp = this.db.SYS_MedicalProcedure.SingleOrDefault(mp1 => mp1.Id == formModel.MedicalProcedureId && mp1.Name == formModel.MedicalProcedureName)
                         ?? this.db.SYS_MedicalProcedure.FirstOrDefault(mp1 => mp1.Name == formModel.MedicalProcedureName);

                if (mp != null)
                {
                    // This means that the user selected something that is in the SYS_MedicalProcedure.
                    dbObject.MedicalProcedureCode = mp.Code;
                    dbObject.MedicalProcedureName = mp.Name;
                }
                else
                {
                    // This means that user edited the procedure name to something that is not in the SYS_MedicalProcedure.
                    dbObject.MedicalProcedureCode = null;
                    dbObject.MedicalProcedureName = formModel.MedicalProcedureName;

                    this.ModelState.Remove(() => formModel.MedicalProcedureId);
                }
            }

            if (this.ModelState.IsValid)
            {
                db.SaveChanges();

                return View("Details", GetViewModel(dbObject));
            }

            return View("Edit", formModel);
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
