using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Areas.App.Models;
using Cerebello.Model;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ExamResultsController : DoctorController
    {
        //
        // GET: /App/Exams/

        public ActionResult Index()
        {
            return View();
        }

        private ExaminationResultViewModel GetViewModel(ExaminationResult examResult)
        {
            return new ExaminationResultViewModel()
            {
                Id = examResult.Id,
                PatientId = examResult.PatientId,
                Text = examResult.Text,
                Title = examResult.Title,
            };
        }

        public ActionResult Details(int id)
        {
            var practiceId = this.GetCurrentUser().PracticeId;

            var examResult = db.ExaminationResults
                .Where(r => r.Id == id)
                .Where(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId)
                .First();

            return this.View(this.GetViewModel(examResult));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(ExaminationResultViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            ExaminationResultViewModel viewModel = null;

            if (id != null)
            {
                var practiceId = this.GetCurrentUser().PracticeId;

                var modelObj = this.db.ExaminationResults
                    .Where(r => r.Id == id)
                    .Where(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId)
                    .FirstOrDefault();

                // todo: if modelObj is null, we must tell the user that this object does not exist.

                viewModel = this.GetViewModel(modelObj);
            }
            else
            {
                viewModel = new ExaminationResultViewModel()
                {
                    Id = id,
                    PatientId = patientId
                };
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ExaminationResultViewModel formModel)
        {
            ExaminationResult modelObj = null;

            if (formModel.Id == null)
            {
                modelObj = new ExaminationResult
                {
                    CreatedOn = DateTime.UtcNow,
                    PatientId = formModel.PatientId.Value,
                };

                this.db.ExaminationResults.AddObject(modelObj);
            }
            else
            {
                var practiceId = this.GetCurrentUser().PracticeId;

                modelObj = this.db.ExaminationResults
                    .Where(r => r.Id == formModel.Id)
                    .Where(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId)
                    .FirstOrDefault();

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

            if (!string.IsNullOrEmpty(formModel.Text))
                modelObj.Text = formModel.Text;

            if (!string.IsNullOrEmpty(formModel.Title))
                modelObj.Title = formModel.Title;

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
                var practiceId = this.GetCurrentUser().PracticeId;

                var modelObj = db.ExaminationResults
                    .Where(r => r.Id == id)
                    .Where(r => r.Patient.Doctor.Users.FirstOrDefault().PracticeId == practiceId)
                    .FirstOrDefault();

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
