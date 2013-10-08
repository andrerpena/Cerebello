using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AutoMapper;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Helpers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using JetBrains.Annotations;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PatientsController : DoctorController
    {
        private class SessionEvent
        {
            public int Id { get; set; }

            /// <summary>
            /// Date and time expressed in local practice time-zone.
            /// </summary>
            public DateTime LocalDate { get; set; }
        }

        protected static PatientViewModel GetViewModel(PracticeController controller, [NotNull] Patient patient, bool includeSessions, bool includeFutureAppointments, bool includeAddressData = true)
        {
            if (patient == null) throw new ArgumentNullException("patient");

            // Person, address, and patient basic properties.
            var viewModel = new PatientViewModel();
            Mapper.Map(patient.Person, viewModel);

            viewModel.Id = patient.Id;
            viewModel.PatientId = patient.Id;
            viewModel.PersonId = patient.Person.Id;
            viewModel.Notes = patient.Person.Notes;

            if (includeAddressData)
            {
                var address = patient.Person.Address;
                if (address != null)
                    Mapper.Map(address, viewModel.Address);
            }

            // Other (more complex) properties.

            if (includeFutureAppointments)
            {
                // gets a textual date. The input date must be LOCAL
                Func<DateTime, string> getRelativeDate = s =>
                {
                    var result = s.ToShortDateString();
                    result += ", " + DateTimeHelper.GetFormattedTime(s);
                    result += ", " +
                              DateTimeHelper.ConvertToRelative(s, controller.GetPracticeLocalNow(),
                                                               DateTimeHelper.RelativeDateOptions.IncludeSuffixes |
                                                               DateTimeHelper.RelativeDateOptions.IncludePrefixes |
                                                               DateTimeHelper.RelativeDateOptions.ReplaceToday |
                                                               DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow);

                    return result;
                };

                // get appointments scheduled for the future
                var utcNow = controller.GetUtcNow();

                var appointments = patient.Appointments
                    .Where(
                        a => a.DoctorId == patient.DoctorId
                             && a.Start > utcNow)
                    .ToList();

                viewModel.FutureAppointments = (from a in appointments
                                                select new AppointmentViewModel
                                                {
                                                    PatientId = a.PatientId,
                                                    PatientFullName = a.PatientId != default(int) ? PersonHelper.GetFullName(a.Patient.Person) : null,
                                                    LocalDateTime = ModelDateTimeHelper.ConvertToLocalDateTime(controller.DbPractice, a.Start),
                                                    LocalDateTimeSpelled = getRelativeDate(ModelDateTimeHelper.ConvertToLocalDateTime(controller.DbPractice, a.Start))
                                                }).ToList();
            }

            if (includeSessions)
            {
                var sessions = GetSessionViewModels(controller.DbPractice, patient, null);

                viewModel.Sessions = sessions;
            }

            return viewModel;
        }

        public static List<SessionViewModel> GetSessionViewModels(Practice practice, Patient patient, DateTimeInterval? filterUtcInterval)
        {
            var eventDates = new List<DateTime>();

            var utcDateFilterStart = filterUtcInterval.HasValue ? filterUtcInterval.Value.Start : (DateTime?)null;
            var utcDateFilterEnd = filterUtcInterval.HasValue ? filterUtcInterval.Value.End : (DateTime?)null;

            // anamneses
            var anamneses = filterUtcInterval.HasValue
                ? patient.PastMedicalHistories.Where(x => x.MedicalRecordDate >= utcDateFilterStart && x.MedicalRecordDate < utcDateFilterEnd)
                : patient.PastMedicalHistories;
            var anamnesesByDate =
                (from avm in
                     (from r in anamneses
                      select new SessionEvent
                                 {
                                     LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, r.MedicalRecordDate),
                                     Id = r.Id
                                 })
                 group avm by avm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(anamnesesByDate.Keys);

            // physical examinations
            var physicalExaminations = filterUtcInterval.HasValue
                ? patient.PhysicalExaminations.Where(x => x.MedicalRecordDate >= utcDateFilterStart && x.MedicalRecordDate < utcDateFilterEnd)
                : patient.PhysicalExaminations;
            var physicalExaminationsByDate =
                (from pe in
                     (from r in physicalExaminations
                      select new SessionEvent
                      {
                          LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, r.MedicalRecordDate),
                          Id = r.Id
                      })
                 group pe by pe.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(physicalExaminationsByDate.Keys);

            // diagnostic hipotheses
            var diagnosticHypotheses = filterUtcInterval.HasValue
                ? patient.DiagnosticHypotheses.Where(x => x.MedicalRecordDate >= utcDateFilterStart && x.MedicalRecordDate < utcDateFilterEnd)
                : patient.DiagnosticHypotheses;
            var diagnosticHypothesesByDate =
                (from pe in
                     (from r in diagnosticHypotheses
                      select new SessionEvent
                      {
                          LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, r.MedicalRecordDate),
                          Id = r.Id
                      })
                 group pe by pe.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(diagnosticHypothesesByDate.Keys);

            // receipts
            var receipts = filterUtcInterval.HasValue
                ? patient.Receipts.Where(x => x.IssuanceDate >= utcDateFilterStart && x.IssuanceDate < utcDateFilterEnd)
                : patient.Receipts;
            var receiptsByDate =
                (from rvm in
                     (from r in receipts
                      select new SessionEvent
                                 {
                                     LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, r.IssuanceDate),
                                     Id = r.Id
                                 })
                 group rvm by rvm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(receiptsByDate.Keys);

            // certificates
            var certificates = filterUtcInterval.HasValue
                ? patient.MedicalCertificates.Where(x => x.IssuanceDate >= utcDateFilterStart && x.IssuanceDate < utcDateFilterEnd)
                : patient.MedicalCertificates;
            var certificatesByDate =
                (from cvm in
                     (from c in certificates
                      select new SessionEvent
                                 {
                                     LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, c.IssuanceDate),
                                     Id = c.Id
                                 })
                 group cvm by cvm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(certificatesByDate.Keys);

            // exam requests
            var examRequests = filterUtcInterval.HasValue
                ? patient.ExaminationRequests.Where(x => x.RequestDate >= utcDateFilterStart && x.RequestDate < utcDateFilterEnd)
                : patient.ExaminationRequests;
            var examRequestsByDate =
                (from ervm in
                     (from c in examRequests
                      select new SessionEvent
                                 {
                                     LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, c.RequestDate),
                                     Id = c.Id
                                 })
                 group ervm by ervm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(examRequestsByDate.Keys);

            // exam results
            var examResults = filterUtcInterval.HasValue
                ? patient.ExaminationResults.Where(x => x.ReceiveDate >= utcDateFilterStart && x.ReceiveDate < utcDateFilterEnd)
                : patient.ExaminationResults;
            var examResultsByDate =
                (from ervm in
                     (from c in examResults
                      select new SessionEvent
                                 {
                                     LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, c.ReceiveDate),
                                     Id = c.Id
                                 })
                 group ervm by ervm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(examResultsByDate.Keys);

            // diagnosis
            var diagnosis = filterUtcInterval.HasValue
                ? patient.Diagnoses.Where(x => x.MedicalRecordDate >= utcDateFilterStart && x.MedicalRecordDate < utcDateFilterEnd)
                : patient.Diagnoses;
            var diagnosisByDate =
                (from dvm in
                     (from d in diagnosis
                      select new SessionEvent
                                 {
                                     LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, d.MedicalRecordDate),
                                     Id = d.Id
                                 })
                 group dvm by dvm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(diagnosisByDate.Keys);

            // patientFiles
            var patientFiles = filterUtcInterval.HasValue
                ? patient.PatientFileGroups.Where(x => x.ReceiveDate >= utcDateFilterStart && x.ReceiveDate < utcDateFilterEnd)
                : patient.PatientFileGroups;
            var patientFilesByDate =
                (from dvm in
                     (from d in patientFiles
                      select new SessionEvent
                      {
                          LocalDate = ModelDateTimeHelper.ConvertToLocalDateTime(practice, d.ReceiveDate),
                          Id = d.Id
                      })
                 group dvm by dvm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(patientFilesByDate.Keys);

            // discover what dates have events
            eventDates = eventDates.Distinct().OrderBy(dt => dt).ToList();

            // creating sessions
            var sessions = eventDates.Select(
                eventDate => new SessionViewModel
                                 {
                                     PatientId = patient.Id,
                                     Date = eventDate,
                                     AnamneseIds =
                                         anamnesesByDate.ContainsKey(eventDate)
                                             ? anamnesesByDate[eventDate].Select(a => a.Id).ToList()
                                             : new List<int>(),
                                     PhysicalExaminationIds =
                                        physicalExaminationsByDate.ContainsKey(eventDate)
                                            ? physicalExaminationsByDate[eventDate].Select(a => a.Id).ToList()
                                    : new List<int>(),
                                     DiagnosticHipothesesId =
                                     diagnosticHypothesesByDate.ContainsKey(eventDate)
                                     ? diagnosticHypothesesByDate[eventDate].Select(a => a.Id).ToList()
                                     : new List<int>(),
                                     ReceiptIds =
                                         receiptsByDate.ContainsKey(eventDate)
                                             ? receiptsByDate[eventDate].Select(v => v.Id).ToList()
                                             : new List<int>(),
                                     MedicalCertificateIds =
                                         certificatesByDate.ContainsKey(eventDate)
                                             ? certificatesByDate[eventDate].Select(c => c.Id).ToList()
                                             : new List<int>(),
                                     ExaminationRequestIds =
                                         examRequestsByDate.ContainsKey(eventDate)
                                             ? examRequestsByDate[eventDate].Select(v => v.Id).ToList()
                                             : new List<int>(),
                                     ExaminationResultIds =
                                         examResultsByDate.ContainsKey(eventDate)
                                             ? examResultsByDate[eventDate].Select(v => v.Id).ToList()
                                             : new List<int>(),
                                     DiagnosisIds =
                                         diagnosisByDate.ContainsKey(eventDate)
                                             ? diagnosisByDate[eventDate].Select(v => v.Id).ToList()
                                             : new List<int>(),
                                     PatientFiles =
                                         patientFilesByDate.ContainsKey(eventDate)
                                             ? patientFilesByDate[eventDate].Select(v => v.Id).ToList()
                                             : new List<int>()
                                 }).ToList();

            return sessions;
        }

        //
        // GET: /App/Patients/

        [CanAlternateUser]
        public ActionResult Index()
        {
            var model =
                new PatientsIndexViewModel
                    {
                        LastRegisteredPatients =
                            (from p in
                                 (from Patient patient in this.db.Patients
                                  where patient.DoctorId == this.Doctor.Id
                                  orderby patient.Person.CreatedOn descending
                                  select patient).Take(Constants.RECENTLY_REGISTERED_LIST_MAXSIZE).ToList()
                             select
                                 new PatientViewModel
                                     {
                                         Id = p.Id,
                                         LastName = p.Person.LastName,
                                         FirstName = p.Person.FirstName,
                                         Gender = p.Person.Gender,
                                         DateOfBirth = ModelDateTimeHelper.ConvertToLocalDateTime(this.DbPractice, p.Person.DateOfBirth),
                                         RecordNumber = p.RecordNumber
                                     }).ToList(),
                        TotalPatientsCount = this.db.Patients.Count(p => p.DoctorId == this.Doctor.Id)
                    };

            // The view must know about the patients limit.
            this.ViewBag.PatientsLimit = this.DbPractice.AccountContract.PatientsLimit;
            this.ViewBag.PatientsCount = this.db.Patients.Count(p => p.PracticeId == this.DbPractice.Id);

            return this.View(model);
        }

        public ActionResult Details(int id)
        {
            var patient = this.db.Patients.SingleOrDefault(p => p.Id == id && p.DoctorId == this.Doctor.Id);

            if (patient == null)
                return new StatusCodeResult(HttpStatusCode.NotFound, "Patient not found.");

            // Only the doctor and the patient can see the medical records.
            var canAccessMedicalRecords = this.DbUser.Id == patient.Doctor.Users.Single().Id;
            this.ViewBag.CanAccessMedicalRecords = canAccessMedicalRecords;

            // Creating the view-model object.
            var model = GetViewModel(this, patient, canAccessMedicalRecords, true);

            this.ViewBag.RecordDate = this.GetPracticeLocalNow().Date;

            return this.View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult Create(PatientViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            PatientViewModel viewModel = null;

            if (id != null)
            {
                // editing an existing patient
                var patient = this.db.Patients.First(p => p.Id == id);
                viewModel = GetViewModel(this, patient, false, false);

                ViewBag.Title = "Editing patient: " + PersonHelper.GetFullName(viewModel.FirstName, viewModel.MiddleName, viewModel.LastName);
            }
            else
            {
                // adding new patient
                this.ViewBag.Title = "Novo paciente";
            }

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(PatientViewModel formModel)
        {
            if (ModelState.IsValid)
            {
                Patient patient;
                if (formModel.Id != null)
                    patient = this.db.Patients.First(p => p.Id == formModel.Id);
                else
                {
                    patient = new Patient
                                  {
                                      Person = new Person { PracticeId = this.DbUser.PracticeId, CreatedOn = this.GetUtcNow() },
                                      PracticeId = this.DbUser.PracticeId,
                                      Doctor = this.Doctor
                                  };
                    this.db.Patients.AddObject(patient);
                }

                // copies all properties from the formModel to the model
                Mapper.Map(formModel, patient.Person);
                patient.IsBackedUp = false;

                // handle patient address
                var patientAddress = patient.Person.Address;
                if (patientAddress == null)
                {
                    patientAddress = new Address
                        {
                            PracticeId = this.DbUser.PracticeId
                        };
                    patient.Person.Address = patientAddress;
                }

                // copies all properties from the formModel to the model (now for the address)
                Mapper.Map(formModel.Address, patientAddress);

                this.db.SaveChanges();
                return this.RedirectToAction("Details", new { id = patient.Id });
            }

            return this.View("Edit", formModel);
        }

        /// <summary>
        /// Deletes a patient
        /// </summary>
        /// <remarks>
        /// Requiriments:
        ///     - Should delete the patient along with the following associations:
        ///         - Anamneses
        /// </remarks>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var patient = this.db.Patients.First(m => m.Id == id);

                // delete anamneses manually
                var anamneses = patient.PastMedicalHistories.ToList();
                while (anamneses.Any())
                {
                    var anamnese = anamneses.First();
                    this.db.PastMedicalHistories.DeleteObject(anamnese);
                    anamneses.Remove(anamnese);
                }

                // delete diagnostic hipotheses manually
                var diagnosticHypotheses = patient.DiagnosticHypotheses.ToList();
                while (diagnosticHypotheses.Any())
                {
                    var diagnosticHypothesis = diagnosticHypotheses.First();
                    this.db.DiagnosticHypotheses.DeleteObject(diagnosticHypothesis);
                    diagnosticHypotheses.Remove(diagnosticHypothesis);
                }

                // delete receipts manually
                var receipts = patient.Receipts.ToList();
                while (receipts.Any())
                {
                    var receipt = receipts.First();
                    this.db.Receipts.DeleteObject(receipt);
                    receipts.Remove(receipt);
                }

                // delete physical exams requests manually
                var physicalExams = patient.PhysicalExaminations.ToList();
                while (physicalExams.Any())
                {
                    var physicalExam = physicalExams.First();
                    this.db.PhysicalExaminations.DeleteObject(physicalExam);
                    physicalExams.Remove(physicalExam);
                }

                // delete exam requests manually
                var examRequests = patient.ExaminationRequests.ToList();
                while (examRequests.Any())
                {
                    var examRequest = examRequests.First();
                    this.db.ExaminationRequests.DeleteObject(examRequest);
                    examRequests.Remove(examRequest);
                }

                // delete exam results manually
                var examResults = patient.ExaminationResults.ToList();
                while (examResults.Any())
                {
                    var examResult = examResults.First();
                    this.db.ExaminationResults.DeleteObject(examResult);
                    examResults.Remove(examResult);
                }

                // delete medical certificates manually
                var certificates = patient.MedicalCertificates.ToList();
                while (certificates.Any())
                {
                    var certificate = certificates.First();

                    // deletes fields within the certificate manually
                    while (certificate.Fields.Any())
                    {
                        var field = certificate.Fields.First();
                        this.db.MedicalCertificateFields.DeleteObject(field);
                    }

                    this.db.MedicalCertificates.DeleteObject(certificate);
                    certificates.Remove(certificate);
                }

                // delete diagnosis manually
                var diagnosis = patient.Diagnoses.ToList();
                while (diagnosis.Any())
                {
                    var diag = diagnosis.First();
                    this.db.Diagnoses.DeleteObject(diag);
                    diagnosis.Remove(diag);
                }

                // delete appointments manually
                var appointments = patient.Appointments.ToList();
                while (appointments.Any())
                {
                    var appointment = appointments.First();
                    this.db.Appointments.DeleteObject(appointment);
                    appointments.Remove(appointment);
                }

                // delete files manually
                var patientFiles = patient.PatientFiles.ToList();
                while (patientFiles.Any())
                {
                    var patientFile = patientFiles.First();
                    var file = patientFile.FileMetadata;

                    var storage = new WindowsAzureBlobStorageManager();
                    storage.DeleteFileFromStorage(Constants.AZURE_STORAGE_PATIENT_FILES_CONTAINER_NAME, file.SourceFileName);

                    this.db.PatientFiles.DeleteObject(patientFile);
                    this.db.FileMetadatas.DeleteObject(file);

                    patientFiles.Remove(patientFile);
                }

                // delete file groups manually
                var patientFileGroups = patient.PatientFileGroups.ToList();
                while (patientFileGroups.Any())
                {
                    var patientFileGroup = patientFileGroups.First();
                    this.db.PatientFileGroups.DeleteObject(patientFileGroup);
                    patientFileGroups.Remove(patientFileGroup);
                }

                this.db.Patients.DeleteObject(patient);
                this.db.SaveChanges();
                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LookupPatients(string term, int pageSize, int? pageIndex)
        {
            if (pageIndex == null)
                throw new ArgumentNullException("pageIndex");

            var baseQuery = this.db.Patients.Include("Person").Where(l => l.DoctorId == this.Doctor.Id);
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(l => l.Person.FirstName.Contains(term) || l.Person.MiddleName.Contains(term) || l.Person.LastName.Contains(term));

            var rows = (from p in baseQuery.OrderBy(p => p.Person.LastName).ThenBy(p => p.Person.FirstName).Skip((pageIndex.Value - 1) * pageSize).Take(pageSize).ToList()
                        select new
                        {
                            Id = p.Id,
                            Value = PersonHelper.GetFullName(p.Person),
                            InsuranceId = p.LastUsedHealthInsuranceId,
                            InsuranceName = this.db.HealthInsurances.Where(hi => hi.Id == p.LastUsedHealthInsuranceId).Select(hi => hi.Name),
                        }).ToList();

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(rows),
                Count = baseQuery.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Searchs for patients
        /// </summary>
        /// <remarks>
        /// Requirements:
        ///     - Should return a slice of the existing records matching the criteria corresponding to the specified 'Page' 
        ///     - Should return a result correspoding to all records when no search term is provided (respecting pagination)
        ///     - In the result, the 'Count' property should consider all records
        ///     - In the result, the list should bring only results corresponding to the specified page
        /// </remarks>
        public ActionResult Search(SearchModel searchModel)
        {
            var model = new SearchViewModel<PatientViewModel>();

            var query = from patient in this.db.Patients.Include("Person")
                        where patient.DoctorId == this.Doctor.Id
                        select patient;

            if (!string.IsNullOrEmpty(searchModel.Term))
                query = from patient in query
                        where patient.Person.FirstName.Contains(searchModel.Term)
                            || patient.Person.MiddleName.Contains(searchModel.Term)
                            || patient.Person.LastName.Contains(searchModel.Term)

                        select patient;

            // 1-based page index
            var pageIndex = searchModel.Page;
            const int pageSize = Constants.GRID_PAGE_SIZE;

            model.Count = query.Count();
            model.Objects = (from p in query.ToList()
                             select new PatientViewModel()
                             {
                                 Id = p.Id,
                                 // Note: this date is coming from the DB in Utc format, and must be converted to local time.
                                 DateOfBirth = p.Person.DateOfBirth,
                                 FirstName = p.Person.FirstName,
                                 LastName = p.Person.LastName
                             })
                             .OrderBy(p => p.LastName)
                             .ThenBy(p => p.FirstName)
                             .Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            // Converting all dates from Utc to local practice time-zone.
            foreach (var eachPatientViewModel in model.Objects)
                eachPatientViewModel.DateOfBirth = ModelDateTimeHelper.ConvertToLocalDateTime(this.DbPractice, eachPatientViewModel.DateOfBirth);

            return this.View(model);
        }

        [SelfPermission]
        public ActionResult AddMedicalRecords(int id, int? y, int? m, int? d)
        {
            var patient = this.db.Patients.SingleOrDefault(p => p.Id == id && p.DoctorId == this.Doctor.Id);

            if (patient == null)
                return new StatusCodeResult(HttpStatusCode.NotFound, "Patient not found");

            // Only the doctor and the patient can see the medical records.
            var canAccessMedicalRecords = this.DbUser.Id == patient.Doctor.Users.Single().Id;
            this.ViewBag.CanAccessMedicalRecords = canAccessMedicalRecords;

            var localDateFilter = DateTimeHelper.CreateDate(y, m, d) ?? this.GetPracticeLocalNow().Date;
            var utcDateFilter = ModelDateTimeHelper.ConvertToUtcDateTime(this.DbPractice, localDateFilter);

            // Creating the view-model object.
            var model = GetViewModel(this, patient, false, false, false);
            model.Sessions = GetSessionViewModels(this.DbPractice, patient, DateTimeInterval.FromDateAndDays(utcDateFilter, 1));

            this.ViewBag.RecordDate = localDateFilter;

            return this.View(model);
        }

        [HttpGet]
        public ActionResult GetDatesWithMedicalRecords(int patientId, int year, int month)
        {
            var localFirst = new DateTime(year, month, 1);
            var localLast = localFirst.AddMonths(1);

            var patient = this.db.Patients.SingleOrDefault(p => p.Id == patientId);

            if (patient == null)
                return new StatusCodeResult(HttpStatusCode.NotFound);

            var utcFirst = ModelDateTimeHelper.ConvertToUtcDateTime(this.DbPractice, localFirst);
            var utcLast = ModelDateTimeHelper.ConvertToUtcDateTime(this.DbPractice, localLast);

            var result = GetSessionViewModels(this.DbPractice, patient, new DateTimeInterval(utcFirst, utcLast))
                .Select(s => s.Date.ToString("'d'dd_MM_yyyy"))
                .Distinct().ToArray();

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailsBasicInformation(int id)
        {
            var patient = this.db.Patients.SingleOrDefault(p => p.Id == id);
            if (patient == null)
                return new StatusCodeResult(HttpStatusCode.NotFound);
            var viewModel = Mapper.Map<PatientBasicInformationViewModel>(patient);
            return this.View(viewModel);
        }

        [HttpGet]
        public ActionResult EditBasicInformation(int id)
        {
            var patient = this.db.Patients.SingleOrDefault(p => p.Id == id);
            if (patient == null)
                return new StatusCodeResult(HttpStatusCode.NotFound);
            var viewModel = Mapper.Map<PatientBasicInformationViewModel>(patient);
            return this.View(viewModel);
        }

        public ActionResult EditBasicInformation(PatientBasicInformationViewModel formModel)
        {
            if (ModelState.IsValid)
            {
                var patient = this.db.Patients.FirstOrDefault(p => p.Id == formModel.Id);
                if (patient == null)
                    return new StatusCodeResult(HttpStatusCode.NotFound);

                // copies all properties from the formModel to the model
                Mapper.Map(formModel, patient);
                patient.IsBackedUp = false;

                this.db.SaveChanges();
                return this.View("DetailsBasicInformation", formModel);
            }

            return this.View(formModel);
        }
    }
}
