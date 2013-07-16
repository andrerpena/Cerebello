using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PhysicalExaminationController : DoctorController
    {
        public static PhysicalExaminationViewModel GetViewModel(PhysicalExamination physicalExamination, Func<DateTime, DateTime> toLocal)
        {
            if (physicalExamination == null)
                return new PhysicalExaminationViewModel();

            return new PhysicalExaminationViewModel
                {
                    Id = physicalExamination.Id,
                    PatientId = physicalExamination.PatientId,
                    Notes = physicalExamination.Notes,
                    MedicalRecordDate = toLocal(physicalExamination.MedicalRecordDate),
                };
        }

        public ActionResult Details(int id)
        {
            var physicalExamination = this.db.PhysicalExaminations.First(pe => pe.Id == id);
            return this.View(GetViewModel(physicalExamination, this.GetToLocalDateTimeConverter()));
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey, int? y, int? m, int? d)
        {
            return this.Edit(null, patientId, y, m, d);
        }

        [HttpPost]
        public ActionResult Create(PhysicalExaminationViewModel[] physicalExaminations)
        {
            return this.Edit(physicalExaminations);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId, int? y, int? m, int? d)
        {
            PhysicalExaminationViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel(
                    (from r in this.db.PhysicalExaminations where r.Id == id select r).First(),
                    this.GetToLocalDateTimeConverter());
            else
                viewModel = new PhysicalExaminationViewModel()
                {
                    Id = null,
                    PatientId = patientId,
                    MedicalRecordDate = DateTimeHelper.CreateDate(y, m, d) ?? this.GetPracticeLocalNow(),
                };

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(PhysicalExaminationViewModel[] physicalExaminations)
        {
            var formModel = physicalExaminations.Single();

            PhysicalExamination physicalExamination;

            if (formModel.Id == null)
            {
                Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
                physicalExamination = new PhysicalExamination
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };
                this.db.PhysicalExaminations.AddObject(physicalExamination);
            }
            else
                physicalExamination = this.db.PhysicalExaminations.FirstOrDefault(pe => pe.Id == formModel.Id);

            if (this.ModelState.IsValid)
            {
                Debug.Assert(physicalExamination != null, "physicalExamination != null");
                physicalExamination.Patient.IsBackedUp = false;
                physicalExamination.Notes = formModel.Notes;
                physicalExamination.MedicalRecordDate = this.ConvertToUtcDateTime(formModel.MedicalRecordDate.Value);
                this.db.SaveChanges();

                return this.View("Details", GetViewModel(physicalExamination, this.GetToLocalDateTimeConverter()));
            }

            return this.View("Edit", GetViewModel(physicalExamination, this.GetToLocalDateTimeConverter()));
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            var physicalExamination = this.db.PhysicalExaminations.First(m => m.Id == id);
            try
            {
                this.db.PhysicalExaminations.DeleteObject(physicalExamination);
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}