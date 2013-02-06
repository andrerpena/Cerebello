using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;
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

        protected static PatientViewModel GetViewModel(PracticeController controller, [NotNull] Patient patient, bool includeSessions, bool includeFutureAppointments)
        {
            if (patient == null) throw new ArgumentNullException("patient");

            // Person, address, and patient basic properties.
            var viewModel = new PatientViewModel();

            FillPersonViewModel(controller.DbPractice, patient.Person, viewModel);

            var address = patient.Person.Addresses.Single();
            viewModel.Id = patient.Id;
            viewModel.Observations = patient.Person.Observations;
            viewModel.Address = GetAddressViewModel(address);

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
                                                    PatientName = a.PatientId != default(int) ? a.Patient.Person.FullName : null,
                                                    LocalDateTime = ConvertToLocalDateTime(controller.DbPractice, a.Start),
                                                    LocalDateTimeSpelled = getRelativeDate(ConvertToLocalDateTime(controller.DbPractice, a.Start))
                                                }).ToList();
            }

            if (includeSessions)
            {
                var sessions = GetSessionViewModels(controller.DbPractice, patient);

                viewModel.Sessions = sessions;
            }

            return viewModel;
        }

        public static AddressViewModel GetAddressViewModel(Address address)
        {
            if (address == null)
                return null;

            return new AddressViewModel
                       {
                           CEP = address.CEP,
                           City = address.City,
                           Complement = address.Complement,
                           Neighborhood = address.Neighborhood,
                           StateProvince = address.StateProvince,
                           Street = address.Street,
                       };
        }

        public static void FillPersonViewModel(Practice practice, Person person, PersonViewModel viewModel)
        {
            viewModel.BirthPlace = person.BirthPlace;
            viewModel.FullName = person.FullName;
            viewModel.Gender = person.Gender;
            viewModel.MaritalStatus = person.MaritalStatus;
            viewModel.CpfOwner = person.CPFOwner;
            viewModel.DateOfBirth = ConvertToLocalDateTime(practice, person.DateOfBirth);
            viewModel.Profissao = person.Profession;
            viewModel.Cpf = person.CPF;
            viewModel.Email = person.Email;
            viewModel.PhoneCell = person.PhoneCell;
            viewModel.PhoneLand = person.PhoneLand;
        }

        public static List<SessionViewModel> GetSessionViewModels(Practice practice, Patient patient)
        {
            var eventDates = new List<DateTime>();

            // anamneses
            var anamnesesByDate =
                (from avm in
                     (from r in patient.Anamneses
                      select new SessionEvent
                                 {
                                     LocalDate = ConvertToLocalDateTime(practice, r.CreatedOn),
                                     Id = r.Id
                                 })
                 group avm by avm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(anamnesesByDate.Keys);

            // physical examinations
            var physicalExaminationsByDate =
                (from pe in
                     (from r in patient.PhysicalExaminations
                      select new SessionEvent
                      {
                          LocalDate = ConvertToLocalDateTime(practice, r.CreatedOn),
                          Id = r.Id
                      })
                 group pe by pe.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(physicalExaminationsByDate.Keys);

            // receipts
            var receiptsByDate =
                (from rvm in
                     (from r in patient.Receipts
                      select new SessionEvent
                                 {
                                     LocalDate = ConvertToLocalDateTime(practice, r.CreatedOn),
                                     Id = r.Id
                                 })
                 group rvm by rvm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(receiptsByDate.Keys);

            // certificates
            var certificatesByDate =
                (from cvm in
                     (from c in patient.MedicalCertificates
                      select new SessionEvent
                                 {
                                     LocalDate = ConvertToLocalDateTime(practice, c.CreatedOn),
                                     Id = c.Id
                                 })
                 group cvm by cvm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(certificatesByDate.Keys);

            // exam requests
            var examRequestsByDate =
                (from ervm in
                     (from c in patient.ExaminationRequests
                      select new SessionEvent
                                 {
                                     LocalDate = ConvertToLocalDateTime(practice, c.CreatedOn),
                                     Id = c.Id
                                 })
                 group ervm by ervm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(examRequestsByDate.Keys);

            // exam results
            var examResultsByDate =
                (from ervm in
                     (from c in patient.ExaminationResults
                      select new SessionEvent
                                 {
                                     LocalDate = ConvertToLocalDateTime(practice, c.CreatedOn),
                                     Id = c.Id
                                 })
                 group ervm by ervm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(examResultsByDate.Keys);

            // diagnosis
            var diagnosisByDate =
                (from dvm in
                     (from d in patient.Diagnoses
                      select new SessionEvent
                                 {
                                     LocalDate = ConvertToLocalDateTime(practice, d.CreatedOn),
                                     Id = d.Id
                                 })
                 group dvm by dvm.LocalDate.Date
                     into g
                     select g).ToDictionary(g => g.Key, g => g.ToList());

            eventDates.AddRange(diagnosisByDate.Keys);

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
                                             : new List<int>()
                                 }).ToList();
            return sessions;
        }

        //
        // GET: /App/Patients/

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
                                  select patient).Take(Constants.LAST_REGISTERED_OBJECTS_COUNT).ToList()
                             select
                                 new PatientViewModel
                                     {
                                         Id = p.Id,
                                         DateOfBirth =
                                             ConvertToLocalDateTime(this.DbPractice, p.Person.DateOfBirth),
                                         FullName = p.Person.FullName
                                     }).ToList(),
                        TotalPatientsCount = this.db.Patients.Count(p => p.DoctorId == this.Doctor.Id)
                    };

            return View(model);
        }

        public ActionResult Details(int id)
        {
            var patient = this.db.Patients.Single(p => p.Id == id);

            // Only the doctor and the patient can see the medical records.
            var canAccessMedicalRecords = this.DbUser.Id == patient.Doctor.Users.Single().Id;
            this.ViewBag.CanAccessMedicalRecords = canAccessMedicalRecords;

            // Creating the view-model object.
            var model = GetViewModel(this, patient, canAccessMedicalRecords, true);

            return View(model);
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
                var patient = this.db.Patients.First(p => p.Id == id);
                viewModel = GetViewModel(this, patient, false, false);

                ViewBag.Title = "Alterando paciente: " + viewModel.FullName;
            }
            else
                ViewBag.Title = "Novo paciente";

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(PatientViewModel formModel)
        {
            if (ModelState.IsValid)
            {
                var isEditing = formModel.Id != null;

                Patient patient = null;
                if (isEditing)
                    patient = this.db.Patients.First(p => p.Id == formModel.Id);
                else
                {
                    patient = new Patient
                                  {
                                      Person = new Person { PracticeId = this.DbUser.PracticeId, },
                                      PracticeId = this.DbUser.PracticeId,
                                  };
                    this.db.Patients.AddObject(patient);
                }

                patient.Doctor = this.Doctor;

                patient.Person.BirthPlace = formModel.BirthPlace;
                patient.Person.CPF = formModel.Cpf;
                patient.Person.CPFOwner = formModel.CpfOwner;
                patient.Person.CreatedOn = this.GetUtcNow();
                patient.Person.DateOfBirth = ConvertToUtcDateTime(this.DbPractice, formModel.DateOfBirth);
                patient.Person.FullName = formModel.FullName;
                patient.Person.Gender = (short)formModel.Gender;
                patient.Person.MaritalStatus = formModel.MaritalStatus;
                patient.Person.Observations = formModel.Observations;
                patient.Person.Profession = formModel.Profissao;
                patient.Person.Email = !string.IsNullOrEmpty(formModel.Email) ? formModel.Email.ToLower() : null;
                patient.Person.EmailGravatarHash = GravatarHelper.GetGravatarHash(formModel.Email);
                patient.Person.PhoneLand = formModel.PhoneLand;
                patient.Person.PhoneCell = formModel.PhoneCell;
                // handle patient address
                if (!patient.Person.Addresses.Any())
                    patient.Person.Addresses.Add(
                        new Address
                            {
                                PracticeId = this.DbUser.PracticeId,
                                CEP = formModel.Address.CEP,
                                City = formModel.Address.City,
                                Complement = formModel.Address.Complement,
                                Neighborhood = formModel.Address.Neighborhood,
                                StateProvince = formModel.Address.StateProvince,
                                Street = formModel.Address.Street,
                            });

                db.SaveChanges();
                return RedirectToAction("Details", new { id = patient.Id });
            }

            return View("Edit", formModel);
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
                var patient = db.Patients.First(m => m.Id == id);

                // delete anamneses manually
                var anamneses = patient.Anamneses.ToList();
                while (anamneses.Any())
                {
                    var anamnese = anamneses.First();

                    // deletes diagnoses within the anamnese manually
                    while (anamnese.Symptoms.Any())
                    {
                        var symptom = anamnese.Symptoms.First();
                        this.db.Symptoms.DeleteObject(symptom);
                    }

                    this.db.Anamnese.DeleteObject(anamnese);
                    anamneses.Remove(anamnese);
                }

                // delete receipts manually
                var receipts = patient.Receipts.ToList();
                while (receipts.Any())
                {
                    var receipt = receipts.First();
                    this.db.Receipts.DeleteObject(receipt);
                    receipts.Remove(receipt);
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

                this.db.Patients.DeleteObject(patient);
                this.db.SaveChanges();
                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LookupPatients(string term, int pageSize, int pageIndex)
        {
            var baseQuery = this.db.Patients.Include("Person").Where(l => l.DoctorId == this.Doctor.Id);
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(l => l.Person.FullName.Contains(term));

            var rows = (from p in baseQuery.OrderBy(p => p.Person.FullName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                        select new
                        {
                            Id = p.Id,
                            Value = p.Person.FullName,
                            InsuranceId = p.LastUsedHealthInsuranceId,
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

            var query = from patient in db.Patients.Include("Person")
                        where patient.DoctorId == this.Doctor.Id
                        select patient;

            if (!string.IsNullOrEmpty(searchModel.Term))
                query = from patient in query where patient.Person.FullName.Contains(searchModel.Term) select patient;

            // 1-based page index
            var pageIndex = searchModel.Page;
            var pageSize = Constants.GRID_PAGE_SIZE;

            model.Count = query.Count();
            model.Objects = (from p in query
                             select new PatientViewModel()
                             {
                                 Id = p.Id,
                                 // Note: this date is coming from the DB in Utc format, and must be converted to local time.
                                 DateOfBirth = p.Person.DateOfBirth,
                                 FullName = p.Person.FullName
                             })
                             .OrderBy(p => p.FullName)
                             .Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            // Converting all dates from Utc to local practice time-zone.
            foreach (var eachPatientViewModel in model.Objects)
                eachPatientViewModel.DateOfBirth = ConvertToLocalDateTime(this.DbPractice, eachPatientViewModel.DateOfBirth);

            return View(model);
        }
    }
}
