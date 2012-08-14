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
        //
        // GET: /App/Exams/

        public ActionResult Index()
        {
            return View();
        }

        private ExaminationRequestViewModel GetViewModel(ExaminationRequest examRequest)
        {
            return new ExaminationRequestViewModel()
            {
                Id = examRequest.Id,
                PatientId = examRequest.PatientId,
                Notes = examRequest.Text,
                MedicalProcedureText = examRequest.SYS_MedicalProcedure.Name,
                MedicalProcedureCode = examRequest.SYS_MedicalProcedure.Code
            };
        }

        public ActionResult Details(int id)
        {
            var examRequest = db.ExaminationRequests.Where(a => a.Id == id).First();
            return this.View(this.GetViewModel(examRequest));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(ExaminationRequestViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            ExaminationRequestViewModel viewModel = null;

            // Todo: security issue... must check current user practice againt the practice of the edited objects.

            if (id != null)
            {
                var modelObj = this.db.ExaminationRequests.Where(r => r.Id == id).FirstOrDefault();

                // todo: if modelObj is null, we must tell the user that this object does not exist.

                viewModel = this.GetViewModel(modelObj);
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
        public ActionResult Edit(ExaminationRequestViewModel formModel)
        {
            ExaminationRequest modelObj = null;

            if (formModel.Id == null)
            {
                modelObj = new ExaminationRequest
                {
                    CreatedOn = DateTime.UtcNow,
                    PatientId = formModel.PatientId.Value,
                };

                this.db.ExaminationRequests.AddObject(modelObj);
            }
            else
            {
                modelObj = this.db.ExaminationRequests.Where(r => r.Id == formModel.Id).FirstOrDefault();

                // If modelObj is null, we must tell the user that this object does not exist.
                if (modelObj == null)
                {
                    return View("NotFound", formModel);
                }

                // Security issue... must check current user practice against the practice of the edited objects.
                var currentUser = this.GetCurrentUser();
                if (currentUser.PracticeId != modelObj.Patient.Doctor.Users.FirstOrDefault().PracticeId)
                {
                    return View("NotFound", formModel);
                }
            }

            modelObj.Text = formModel.Notes;

            // Only sets the MedicalProcedureId when MedicalProcedureText is not null.
            if (!string.IsNullOrEmpty(formModel.MedicalProcedureText))
                modelObj.MedicalProcedureId = formModel.MedicalProcedureId;

            if (this.ModelState.IsValid)
            {
                db.SaveChanges();

                return View("details", this.GetViewModel(modelObj));
            }

            return View("edit", formModel);
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
                var practiceId = this.GetCurrentUser().PracticeId;

                var modelObj = db.ExaminationRequests
                    .Where(m => m.Id == id)
                    .Where(m => m.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId)
                    .FirstOrDefault();

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
