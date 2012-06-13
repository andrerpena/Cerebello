using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Models;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using CerebelloWebRole.Code.Controllers;
using System.Data.Objects;
using Cerebello.Model;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Mvc;
using CerebelloWebRole.Code.Controls;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PatientsController : DoctorController
    {
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

                // discover what dates have events
                var eventDates = receiptsByDate.Keys.Concat(anamnesesByDate.Keys).Concat(certificatesByDate.Keys).Distinct().OrderBy(dt => dt);

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
                        MedicalCertificateIds = certificatesByDate.ContainsKey(eventDate) ? certificatesByDate[eventDate].Select(c => c.Id).ToList() : new List<int>()
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
                                  (from Patient patient in db.Patients where patient.DoctorId == this.Doctor.Id select patient).ToList()
                              select new PatientViewModel
                              {
                                  Id = p.Id,
                                  DateOfBirth = p.Person.DateOfBirth,
                                  FullName = p.Person.FullName
                              }).ToList();

            var timeZoneNow = DateTimeHelper.GetTimeZoneNow();

            model.PatientAgeDistribution = (from p in db.Patients where p.DoctorId == this.Doctor.Id group p by new { Gender = p.Person.Gender, Age = EntityFunctions.DiffYears(p.Person.DateOfBirth, timeZoneNow) } into g select g).OrderBy(g => g.Key).Select(g => new CerebelloWebRole.Areas.App.Models.PatientsIndexViewModel.ChartPatientAgeDistribution { Age = g.Key.Age, Gender = g.Key.Gender, Count = g.Count() }).ToList();
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
            PatientViewModel model = null;

            if (id != null)
            {
                var patient = db.Patients.Where(p => p.Id == id).First();
                model = this.GetViewModel(patient);

                ViewBag.Title = "Alterando paciente: " + model.FullName;
            }
            else
                ViewBag.Title = "Novo paciente";

            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(PatientViewModel formModel)
        {
            if (ModelState.IsValid)
            {
                Patient patient = null;

                if (formModel.Id != null)
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
                patient.Person.UrlIdentifier = StringHelper.GenerateUrlIdentifier(formModel.FullName);
                patient.Person.Gender = (short)formModel.Gender;
                patient.Person.MaritalStatus = (short?) formModel.MaritalStatus;
                patient.Person.Observations = formModel.Observations;
                patient.Person.Profession = formModel.Profissao;

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

                db.SaveChanges();

                return RedirectToAction("details", new { id = patient.Id });
            }

            return View("Edit", formModel);
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
                Count = rows.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Search(SearchModel searchModel)
        {
            var model = new DoctorPatientsSearchViewModel();

            var query = from patient in db.Patients where patient.DoctorId == this.Doctor.Id select patient;

            if (!string.IsNullOrEmpty(searchModel.Term))
                query = from patient in query where patient.Person.FullName.Contains(searchModel.Term) select patient;


            model.Patients = (from patient in query.ToList()
                              select this.GetViewModel(patient)).ToList();

            return View(model);
        }

        public ActionResult AddressEditor(AddressViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}
