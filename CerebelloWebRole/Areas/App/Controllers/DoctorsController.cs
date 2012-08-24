﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class DoctorsController : PracticeController
    {
        public DoctorsController()
        {
        }

        //
        // GET: /App/PracticeDoctors/

        public ActionResult Index()
        {
            var model = new PracticeDoctorsViewModel();
            model.Doctors = GetDoctorViewModelsFromPractice(this.db, this.Practice, this.GetPracticeLocalNow());
            return View(model);
        }

        public static List<DoctorViewModel> GetDoctorViewModelsFromPractice(CerebelloEntities db, Practice practice, DateTime localNow)
        {
            var usersThatAreDoctors = db.Users
                .Where(u => u.PracticeId == practice.Id)
                .Where(u => u.Doctor != null);

            var dataCollection = usersThatAreDoctors
                .Select(u => new
                {
                    vm = new DoctorViewModel()
                    {
                        Id = u.Id,
                        Name = u.Person.FullName,
                        UrlIdentifier = u.Person.UrlIdentifier,
                        CRM = u.Doctor.CRM,
                        MedicalSpecialty = u.Doctor.SYS_MedicalSpecialty.Name,
                    },
                    MedicalEntityCode = u.Doctor.SYS_MedicalEntity.Code,
                    u.Doctor.MedicalEntityJurisdiction,
                    doc = u.Doctor,
                    u.GravatarEmailHash,
                })
                .ToList();

            // Getting more doctor's informations:
            // Todo: this is going to become a problem in the future, because this info has no cache.
            // - next free time slot of each doctor;
            // - gravatar image.
            foreach (var eachItem in dataCollection)
            {
                if (!string.IsNullOrEmpty(eachItem.GravatarEmailHash))
                    eachItem.vm.ImageUrl = GravatarHelper.GetGravatarUrl(eachItem.GravatarEmailHash, GravatarHelper.Size.s64);

                eachItem.vm.MedicalEntity = string.Format(
                    string.IsNullOrEmpty(eachItem.MedicalEntityJurisdiction) ? "{0}" : "{0}-{1}",
                    eachItem.MedicalEntityCode,
                    eachItem.MedicalEntityJurisdiction);

                // It is only possible to determine the next available time if the schedule of the doctor is already configured.
                if (eachItem.doc.CFG_Schedule != null)
                {
                    var nextSlotInLocalTime = ScheduleController.FindNextFreeTimeInPracticeLocalTime(db, eachItem.doc, localNow);
                    eachItem.vm.NextAvailableTime = nextSlotInLocalTime.Item1;
                }
            }

            var doctors = dataCollection.Select(item => item.vm).ToList();
            return doctors;
        }
    }
}
