using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Mvc;
using HtmlAgilityPack;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PatientsController : DoctorController
    {
        public PatientsController()
        {
            this.UserNowGetter = () => DateTimeHelper.GetTimeZoneNow();
            this.UtcNowGetter = () => DateTime.UtcNow;
        }

        public Func<DateTime> UserNowGetter { get; set; }

        public Func<DateTime> UtcNowGetter { get; set; }

        public class SessionEvent
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
        }

        private PatientViewModel GetViewModel(Patient patient, bool includeSessions = false)
        {
            var viewModel = new PatientViewModel()
            {
                Id = patient.Id,
                BirthPlace = patient.Person.BirthPlace,
                CoverageId = patient.CoverageId,
                FullName = patient.Person.FullName,
                Gender = patient.Person.Gender,
                MaritalStatus = patient.Person.MaritalStatus,
                Observations = patient.Person.Observations,
                CPFOwner = patient.Person.CPFOwner,
                DateOfBirth = patient.Person.DateOfBirth,
                Profissao = patient.Person.Profession,
                CoverageText = patient.Coverage != null ? patient.Coverage.Name : "",
                CPF = patient.Person.CPF,

                //DoctorId = this.Doctor.Id,

                Emails = (from e in patient.Person.Emails
                          select new EmailViewModel()
                          {
                              Id = e.Id,
                              Address = e.Address
                          }).ToList(),

                Addresses = (from a in patient.Person.Addresses
                             select new AddressViewModel()
                             {
                                 Id = a.Id,
                                 CEP = a.CEP,
                                 City = a.City,
                                 Complement = a.Complement,
                                 Neighborhood = a.Neighborhood,
                                 StateProvince = a.StateProvince,
                                 Street = a.Street
                             }).ToList()
            };

            if (includeSessions)
            {
                var eventDates = new List<DateTime>();

                // anamneses
                var anamnesesByDate =
                                    (from avm in
                                         (from r in patient.Anamneses
                                          select new SessionEvent
                                          {
                                              Date = DateTimeHelper.ConvertToCurrentTimeZone(r.CreatedOn),
                                              Id = r.Id
                                          })
                                     group avm by avm.Date.Date into g
                                     select g).ToDictionary(g => g.Key, g => g.ToList());

                eventDates.AddRange(anamnesesByDate.Keys);

                // receipts
                var receiptsByDate =
                                    (from rvm in
                                         (from r in patient.Receipts
                                          select new SessionEvent
                                          {
                                              Date = DateTimeHelper.ConvertToCurrentTimeZone(r.CreatedOn),
                                              Id = r.Id
                                          })
                                     group rvm by rvm.Date.Date into g
                                     select g).ToDictionary(g => g.Key, g => g.ToList());

                eventDates.AddRange(receiptsByDate.Keys);

                // certificates
                var certificatesByDate =
                                    (from cvm in
                                         (from c in patient.MedicalCertificates
                                          select new SessionEvent
                                          {
                                              Date = DateTimeHelper.ConvertToCurrentTimeZone(c.CreatedOn),
                                              Id = c.Id
                                          })
                                     group cvm by cvm.Date.Date into g
                                     select g).ToDictionary(g => g.Key, g => g.ToList());

                eventDates.AddRange(certificatesByDate.Keys);

                // exam requests
                var examRequestsByDate =
                                 (from ervm in
                                      (from c in patient.ExaminationRequests
                                       select new SessionEvent
                                       {
                                           Date = DateTimeHelper.ConvertToCurrentTimeZone(c.CreatedOn),
                                           Id = c.Id
                                       })
                                  group ervm by ervm.Date.Date into g
                                  select g).ToDictionary(g => g.Key, g => g.ToList());

                eventDates.AddRange(examRequestsByDate.Keys);

                // exam results
                var examResultsByDate =
                                 (from ervm in
                                      (from c in patient.ExaminationResults
                                       select new SessionEvent
                                       {
                                           Date = DateTimeHelper.ConvertToCurrentTimeZone(c.CreatedOn),
                                           Id = c.Id
                                       })
                                  group ervm by ervm.Date.Date into g
                                  select g).ToDictionary(g => g.Key, g => g.ToList());

                eventDates.AddRange(examResultsByDate.Keys);

                // discover what dates have events
                eventDates = eventDates.Distinct().OrderBy(dt => dt).ToList();

                // creating sessions
                List<SessionViewModel> sessions = new List<SessionViewModel>();

                foreach (var eventDate in eventDates)
                {
                    sessions.Add(new SessionViewModel()
                    {
                        PatientId = patient.Id,
                        Date = eventDate,
                        AnamneseIds = anamnesesByDate.ContainsKey(eventDate) ? anamnesesByDate[eventDate].Select(a => a.Id).ToList() : new List<int>(),
                        ReceiptIds = receiptsByDate.ContainsKey(eventDate) ? receiptsByDate[eventDate].Select(v => v.Id).ToList() : new List<int>(),
                        MedicalCertificateIds = certificatesByDate.ContainsKey(eventDate) ? certificatesByDate[eventDate].Select(c => c.Id).ToList() : new List<int>(),
                        ExaminationRequestIds = examRequestsByDate.ContainsKey(eventDate) ? examRequestsByDate[eventDate].Select(v => v.Id).ToList() : new List<int>(),
                        ExaminationResultIds = examResultsByDate.ContainsKey(eventDate) ? examResultsByDate[eventDate].Select(v => v.Id).ToList() : new List<int>(),
                    });
                }

                viewModel.Sessions = sessions;
            }

            return viewModel;
        }

        //
        // GET: /App/Patients/

        public ActionResult Index()
        {
            var model = new PatientsIndexViewModel();
            model.LastRegisteredPatients = (from p in
                                                (from Patient patient in db.Patients
                                                 where patient.DoctorId == this.Doctor.Id
                                                 select patient).ToList()
                                            select new PatientViewModel
                                            {
                                                Id = p.Id,
                                                DateOfBirth = p.Person.DateOfBirth,
                                                FullName = p.Person.FullName
                                            }).ToList();

            var timeZoneNow = DateTimeHelper.GetTimeZoneNow();

            model.PatientAgeDistribution = (from p in db.Patients
                                            where p.DoctorId == this.Doctor.Id
                                            group p by new
                                            {
                                                Gender = p.Person.Gender,
                                                Age = EntityFunctions.DiffYears(p.Person.DateOfBirth, timeZoneNow)
                                            } into g
                                            select g).OrderBy(g => g.Key)
                                            .Select(g => new PatientsIndexViewModel.ChartPatientAgeDistribution
                                            {
                                                Age = g.Key.Age,
                                                Gender = g.Key.Gender,
                                                Count = g.Count()
                                            }).ToList();

            model.TotalPatientsCount = this.db.Patients.Where(p => p.DoctorId == this.Doctor.Id).Count();
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult LastAddedPatients()
        {
            var patients = (from p in db.Patients select p).ToList();
            return View(patients);
        }

        public ActionResult Details(int id)
        {
            var patient = (Patient)db.Patients.Where(p => p.Id == id).First();
            var model = this.GetViewModel(patient, true);

            this.ViewBag.UserNow = this.UserNowGetter();

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
                var patient = db.Patients.Where(p => p.Id == id).First();
                viewModel = this.GetViewModel(patient);

                ViewBag.Title = "Alterando paciente: " + viewModel.FullName;
            }
            else
                ViewBag.Title = "Novo paciente";

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(PatientViewModel formModel)
        {
            Patient patient = null;

            if (ModelState.IsValid)
            {
                bool isEditing = formModel.Id != null;

                if (isEditing)
                    patient = db.Patients.Where(p => p.Id == formModel.Id).First();
                else
                {
                    patient = new Patient();
                    patient.Person = new Person();
                    this.db.Patients.AddObject(patient);
                }

                patient.Doctor = this.Doctor;

                patient.Person.BirthPlace = formModel.BirthPlace;
                patient.Person.CPF = formModel.CPF;
                patient.Person.CPFOwner = formModel.CPFOwner;
                patient.Person.CreatedOn = DateTime.UtcNow;
                patient.Person.DateOfBirth = formModel.DateOfBirth;
                patient.Person.FullName = formModel.FullName;
                patient.Person.Gender = (short)formModel.Gender;
                patient.Person.MaritalStatus = (short?)formModel.MaritalStatus;
                patient.Person.Observations = formModel.Observations;
                patient.Person.Profession = formModel.Profissao;

                // Creating an unique UrlIdentifier for this patient.
                // This does not consider UrlIdentifier's used by the users of the software.
                var practiceId = this.Doctor.Users.FirstOrDefault().PracticeId;

                string urlId = GetUniquePatientUrlId(this.db, formModel.FullName, practiceId);
                if (urlId == null)
                {
                    this.ModelState.AddModelError(
                        () => formModel.FullName,
                        // Todo: this message is also used in the AuthenticationController.
                        "Quantidade máxima de homônimos excedida.");
                }
                patient.Person.UrlIdentifier = urlId;

                patient.Person.Addresses.Update(
                    formModel.Addresses,
                    (vm, m) => vm.Id == m.Id,
                    (vm, m) =>
                    {
                        m.CEP = vm.CEP;
                        m.StateProvince = vm.StateProvince;
                        m.City = vm.City;
                        m.Complement = vm.Complement;
                        m.Neighborhood = vm.Neighborhood;
                        m.StateProvince = vm.StateProvince;
                        m.Street = vm.Street;
                    },
                    (m) => this.db.Addresses.DeleteObject(m)
                );

                patient.Person.Emails.Update(
                    formModel.Emails,
                    (vm, m) => vm.Id == m.Id,
                    (vm, m) =>
                    {
                        m.Address = vm.Address;
                    },
                    (m) => this.db.Emails.DeleteObject(m)
                );
            }

            if (this.ModelState.IsValid)
            {
                db.SaveChanges();

                return RedirectToAction("details", new { id = patient.Id });
            }

            return View("Edit", formModel);
        }

        public static string GetUniquePatientUrlId(CerebelloEntities db, string fullName, int practiceId)
        {
            // todo: this method has been cloned in firestarter

            // Creating an unique UrlIdentifier for this patient.
            // When another patient have the same UrlIdentifier, we try to append a
            // number after the string so that it becomes different, and if it is also used
            // then increment the number and try again.
            var urlIdSrc = StringHelper.GenerateUrlIdentifier(fullName);
            var urlId = urlIdSrc;

            // todo: there is a concurrency problem here.
            int cnt = 2;
            while (db.Patients
                .Where(p => p.Doctor.Users.FirstOrDefault().PracticeId == practiceId)
                .Where(p => p.Person.UrlIdentifier == urlId).Any())
            {
                urlId = string.Format("{0}_{1}", urlIdSrc, cnt++);

                if (cnt > 20)
                    return null;
            }

            return urlId;
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var patient = db.Patients.Where(m => m.Id == id).First();

                // delete receipts manually (SQL Server won't do this automatically)
                var receipts = patient.Receipts.ToList();
                while (receipts.Any())
                {
                    var receipt = receipts.First();
                    this.db.Receipts.DeleteObject(receipt);
                    receipts.Remove(receipt);
                }

                // delete appointments manulally (SQL Server won't do this automatically)
                var appointments = patient.Appointments.ToList();
                while (appointments.Any())
                {
                    var appointment = appointments.First();
                    this.db.Appointments.DeleteObject(appointment);
                    appointments.Remove(appointment);
                }

                // delete anamneses manulally (SQL Server won't do this automatically)
                var anamneses = patient.Anamneses.ToList();
                while (anamneses.Any())
                {
                    var anamnese = anamneses.First();
                    this.db.Anamnese.DeleteObject(anamnese);
                    anamneses.Remove(anamnese);
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

        public JsonResult GetCEPInfo(string cep)
        {
            // TODO: Miguel Angelo: copiei este código para o UsersController, deveria ser um método utilitário, ou criar uma classe de base.

            try
            {
                var request = HttpWebRequest.Create("http://www.buscacep.correios.com.br/servicos/dnec/consultaEnderecoAction.do");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
                    requestWriter.Write(String.Format("relaxation={0}&TipoCep=ALL&semelhante=N&cfm=1&Metodo=listaLogradouro&TipoConsulta=relaxation&StartRow=1&EndRow=10", cep));

                var response = request.GetResponse();

                HtmlDocument document = new HtmlDocument();
                document.Load(response.GetResponseStream());

                CEPInfo cepInfo = new CEPInfo()
                {
                    Street = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[1]").InnerText,
                    Neighborhood = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[2]").InnerText,
                    City = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[3]").InnerText,
                    StateProvince = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[4]").InnerText
                };

                return Json(cepInfo, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return null;
            }
        }

        public JsonResult LookupPatients(string term, int pageSize, int pageIndex)
        {
            var baseQuery = this.db.Patients.Include("Person").Where(l => l.DoctorId == this.Doctor.Id);
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(l => l.Person.FullName.Contains(term));

            var rows = (from p in baseQuery.OrderBy(p => p.Person.FullName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                        select new LookupRow
                        {
                            Id = p.Id,
                            Value = p.Person.FullName
                        }).ToList();

            var result = new LookupJsonResult()
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
                                 DateOfBirth = p.Person.DateOfBirth,
                                 FullName = p.Person.FullName
                             }).OrderBy(p => p.FullName).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return View(model);
        }

        public ActionResult EmailEditor(EmailViewModel viewModel)
        {
            return this.View(viewModel);
        }

        public ActionResult AddressEditor(AddressViewModel viewModel)
        {
            // there seems to be a BUG regarding DropdownListFors inside editor templates
            // and possibly inside collection-editors. This is a workaraound (I'm forcingly
            // setting the value).
            // If we go into this more often we can investigate a more elegant solution.
            ViewBag.UFOptions = new SelectList(new List<SelectListItem>()
                    {
                        new SelectListItem() { Text = "Acre", Value="AC"},
                        new SelectListItem() { Text = "Alagoas", Value="AL"},
                        new SelectListItem() { Text = "Amapá", Value="AP"},
                        new SelectListItem() { Text = "Amazonas", Value="AM"},
                        new SelectListItem() { Text = "Bahia", Value="BA"},
                        new SelectListItem() { Text = "Ceará", Value="CE"},
                        new SelectListItem() { Text = "Distrito Federal", Value="DF"},
                        new SelectListItem() { Text = "Espírito Santo", Value="ES"},
                        new SelectListItem() { Text = "Goiás", Value="GO"},
                        new SelectListItem() { Text = "Maranhão", Value="MA"},
                        new SelectListItem() { Text = "Mato Grosso", Value="MT"},
                        new SelectListItem() { Text = "Mato Grosso do Sul", Value="MS"},
                        new SelectListItem() { Text = "Minas Gerais", Value="MG"},
                        new SelectListItem() { Text = "Pará", Value="PA"},
                        new SelectListItem() { Text = "Paraiba", Value="PB"},
                        new SelectListItem() { Text = "Pernambuco", Value="PE"},
                        new SelectListItem() { Text = "Piauí", Value="PI"},
                        new SelectListItem() { Text = "Rio de Janeiro", Value="RJ"},
                        new SelectListItem() { Text = "Rio Grande do Norte", Value="RN"},
                        new SelectListItem() { Text = "Rio Grande do Sul", Value="RS"},
                        new SelectListItem() { Text = "Rondônia", Value="RO"},
                        new SelectListItem() { Text = "Roraima", Value="RR"},
                        new SelectListItem() { Text = "Santa Catarina", Value="SC"},
                        new SelectListItem() { Text = "São Paulo", Value="SP"},
                        new SelectListItem() { Text = "Sergipe", Value="SE"},
                        new SelectListItem() { Text = "Tocantins", Value="TO"}
                    }, "Value", "Text", viewModel.StateProvince);

            return View(viewModel);
        }
    }
}
