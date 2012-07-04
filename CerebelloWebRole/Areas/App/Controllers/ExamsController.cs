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
                Text = examRequest.Text,
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

            // Todo: security issue... must check current user practice againt the practice of the edited objects.

            if (this.ModelState.IsValid)
            {
                if (formModel.Id == null)
                {
                    modelObj = new ExaminationRequest()
                    {
                        CreatedOn = DateTime.UtcNow,
                        PatientId = formModel.PatientId.Value,
                    };
                    this.db.ExaminationRequests.AddObject(modelObj);
                }
                else
                {
                    modelObj = this.db.ExaminationRequests.Where(r => r.Id == formModel.Id).FirstOrDefault();

                    // todo: if modelObj is null, we must tell the user that this object does not exist.
                }

                modelObj.Text = formModel.Text;

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
        ///     2 - In case the given exam-request doesn't exist, it should return a JsonDeleteMessage with success = false and the text property
        ///         informing the reason of the failure
        /// 
        /// </summary>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var modelObj = db.ExaminationRequests.Where(m => m.Id == id).First();

                this.db.ExaminationRequests.DeleteObject(modelObj);
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
