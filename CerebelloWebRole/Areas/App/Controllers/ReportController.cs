using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Cerebello.Model;
using Cerebello.SmartRecords;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using SmartRecords;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ReportController : DoctorController
    {
        [SelfPermission]
        public ContentResult ExportDoctorXml()
        {
            var xml = ExportDoctorXml(this.db, this.DbPractice, this.Doctor);

            return this.Content(xml, "text/xml");
        }

        internal static string ExportDoctorXml(CerebelloEntitiesAccessFilterWrapper db, Practice practice, Doctor doctor)
        {
            var rg = new ReportData(db, practice);

            var doctorData = rg.GetReportDataSourceForXml(doctor, null);

            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                var xs = new XmlSerializer(typeof(ReportData.XmlDoctorData));
                xs.Serialize(writer, doctorData);
            }

            return stringBuilder.ToString();
        }

        internal static byte[] ExportPatientsPdf(int? patientId, CerebelloEntitiesAccessFilterWrapper db, Practice practice, Doctor doctor)
        {
            var reportDataSource = new ReportData(db, practice).GetReportDataSourceForPdf(doctor, patientId);

            // the frame determines what is displayed in the header and in the
            // footer of your report pages
            var frame = new ReportFrame(
                new ReportFrameData()
                {
                    Header1 = doctor.CFG_Documents.Header1,
                    Header2 = doctor.CFG_Documents.Header2,
                    FooterLeft1 = doctor.CFG_Documents.FooterLeft1,
                    FooterLeft2 = doctor.CFG_Documents.FooterLeft2,
                    FooterRight1 = doctor.CFG_Documents.FooterRight1,
                    FooterRight2 = doctor.CFG_Documents.FooterRight2
                });

            // creates a new report
            var report = new Report(frame);
            foreach (var patient in reportDataSource.Patients)
            {
                // creates a data-context for each contact inside the report.
                // a data context is a section of your report that is bound to
                // a particular data object.
                using (var patientContext = report.AddDataContext(patient))
                {
                    // creates a title for the contact
                    patientContext.AddTitle(ReportTitleSize.H1, m => "Paciente: " + m.FullName);

                    // creates a Card to display the contacts details
                    var card = patientContext.AddCard();
                    card.AddField(m => m.FullName, true);
                    card.AddField(m => m.Gender);
                    card.AddField(m => m.DateOfBirth);
                    card.AddField(m => m.Profissao);
                    card.AddField(m => m.MaritalStatus);
                    card.AddField(m => m.PhoneLand);
                    card.AddField(m => m.PhoneCell);
                    card.AddField(m => m.Email, true);
                    card.AddField(m => m.Observations, true);

                    // creates another Card for displaying the contact's address details
                    patientContext.AddTitle(ReportTitleSize.H2, m => "Endereço");
                    var addressCard = patientContext.AddCard(m => m.Address);
                    addressCard.AddField(m => m.Street, true);
                    addressCard.AddField(m => m.Neighborhood, true);
                    addressCard.AddField(m => m.Complement);
                    addressCard.AddField(m => m.City);
                    addressCard.AddField(m => m.StateProvince);
                    addressCard.AddField(m => m.CEP);

                    for (var i = 0; i < patient.Sessions.Count; i++)
                    {
                        var closureI = i;
                        var session = patient.Sessions[i];

                        patientContext.AddTitle(ReportTitleSize.H2, m => "Consulta do dia " + session.Date.ToShortDateString() + " às " + session.Date.ToShortTimeString());

                        for (var j = 0; j < session.Anamneses.Count; j++)
                        {
                            var closureJ = j;
                            patientContext.AddTitle(ReportTitleSize.H2, m => "Anamnese");
                            var anamneseCard = patientContext.AddCard(c => c.Sessions[closureI].Anamneses[closureJ]);
                            anamneseCard.AddField(m => m.ChiefComplaint, true);
                            anamneseCard.AddField(m => m.HistoryOfThePresentIllness, true);
                            anamneseCard.AddField(m => m.PastMedicalHistory, true);
                            anamneseCard.AddField(m => m.ReviewOfSystems, true);
                            anamneseCard.AddField(m => m.FamilyDeseases, true);
                            anamneseCard.AddField(m => m.SocialHistory, true);
                            anamneseCard.AddField(m => m.RegularAndAcuteMedications, true);
                            anamneseCard.AddField(m => m.Allergies, true);
                            anamneseCard.AddField(m => m.SexualHistory, true);
                            anamneseCard.AddField(m => m.Conclusion, true);
                        }

                        for (var j = 0; j < session.PhysicalExaminations.Count; j++)
                        {
                            var closureJ = j;
                            patientContext.AddTitle(ReportTitleSize.H2, m => "Exame físico");
                            var physicalExaminationCard = patientContext.AddCard(c => c.Sessions[closureI].PhysicalExaminations[closureJ]);
                            physicalExaminationCard.AddField(m => m.Notes);
                        }

                        for (var j = 0; j < session.DiagnosticHipotheses.Count; j++)
                        {
                            var closureJ = j;
                            patientContext.AddTitle(ReportTitleSize.H2, m => "Hipótese diagnóstica");
                            var physicalExaminationCard = patientContext.AddCard(c => c.Sessions[closureI].DiagnosticHipotheses[closureJ]);
                            physicalExaminationCard.AddField(m => m.Cid10Code);
                            physicalExaminationCard.AddField(m => m.Cid10Name);
                            physicalExaminationCard.AddField(m => m.Text);
                        }

                        for (var j = 0; j < session.Prescriptions.Count; j++)
                        {
                            var closureJ = j;
                            patientContext.AddTitle(ReportTitleSize.H2, m => "Receita");
                            var prescriptionGrid = patientContext.AddGrid(c => c.Sessions[closureI].Prescriptions[closureJ].PrescriptionMedicines);
                            prescriptionGrid.AddColumn(m => m.MedicineText);
                            prescriptionGrid.AddColumn(m => m.Quantity);
                            prescriptionGrid.AddColumn(m => m.Prescription);
                            prescriptionGrid.AddColumn(m => m.Observations);
                        }

                        if (session.ExaminationRequests.Any())
                        {
                            patientContext.AddTitle(ReportTitleSize.H2, m => "Pedidos de exame ou procedimento");
                            var examinationRequestsGrid = patientContext.AddGrid(c => c.Sessions[closureI].ExaminationRequests);
                            examinationRequestsGrid.AddColumn(m => m.MedicalProcedureName);
                            examinationRequestsGrid.AddColumn(m => m.MedicalProcedureCode);
                            examinationRequestsGrid.AddColumn(m => m.Notes);
                        }

                        if (session.ExaminationResults.Any())
                        {
                            patientContext.AddTitle(ReportTitleSize.H2, m => "Resultados de exame ou procedimento");
                            var examinationResultGrid = patientContext.AddGrid(c => c.Sessions[closureI].ExaminationResults);
                            examinationResultGrid.AddColumn(m => m.MedicalProcedureName);
                            examinationResultGrid.AddColumn(m => m.MedicalProcedureCode);
                            examinationResultGrid.AddColumn(m => m.Text);
                        }
                    }
                }
            }

            return report.SaveToByteArray();
        }

        [SelfPermission]
        public FileContentResult ExportPatientsPdf(int? patientId)
        {
            var pdf = ExportPatientsPdf(patientId, this.db, this.DbPractice, this.Doctor);

            // Returning the generated PDF as a file.
            return this.File(pdf, "application/pdf");
        }
    }

    public class ReportData
    {
        private readonly CerebelloEntitiesAccessFilterWrapper db;
        private readonly Practice practice;

        public ReportData(CerebelloEntitiesAccessFilterWrapper db, Practice practice)
        {
            this.db = db;
            this.practice = practice;
        }

        public XmlDoctorData GetReportDataSourceForXml(Doctor doctor, int? patientId)
        {
            return this.GetReportDataSource(doctor, patientId, false);
        }

        public PdfDoctorData GetReportDataSourceForPdf(Doctor doctor, int? patientId)
        {
            return (PdfDoctorData)this.GetReportDataSource(doctor, patientId, true);
        }

        private XmlDoctorData GetReportDataSource(Doctor doctor, int? patientId, bool isPdf)
        {
            var docUser = doctor.Users.Single();
            var docPerson = docUser.Person;

            var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doctor);
            var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doctor);

            // Getting all patients data.
            var doctorData = isPdf ? new PdfDoctorData() : new XmlDoctorData();
            PatientsController.FillPersonViewModel(this.practice, docPerson, doctorData);
            UsersController.FillUserViewModel(docUser, this.practice, doctorData);
            UsersController.FillDoctorViewModel(docUser, medicalEntity, medicalSpecialty, doctorData, doctor);

            doctorData.Address = PatientsController.GetAddressViewModel(docPerson.Addresses.SingleOrDefault());

            if (patientId != null)
            {
                doctorData.Patients = doctor.Patients
                                            .Where(p => p.Id == patientId)
                                            .Select(p => this.GetPatientData(p, isPdf))
                                            .OrderBy(x => x.FullName)
                                            .ToList();
            }
            else
            {
                doctorData.Patients = doctor.Patients
                                            .Select(p => this.GetPatientData(p, isPdf))
                                            .OrderBy(x => x.FullName)
                                            .ToList();
            }

            return doctorData;
        }

        private XmlPatientData GetPatientData(Patient patient, bool isPdf)
        {
            var result = isPdf ? new PdfPatientData() : new XmlPatientData();

            PatientsController.FillPersonViewModel(this.practice, patient.Person, result);

            result.Id = patient.Id;
            result.Observations = patient.Person.Observations;

            result.Address = PatientsController.GetAddressViewModel(patient.Person.Addresses.FirstOrDefault())
                ?? new AddressViewModel();

            result.Sessions = this.GetAllSessionsData(patient);

            return result;
        }

        private List<SessionData> GetAllSessionsData(Patient patient)
        {
            var sessions = PatientsController.GetSessionViewModels(this.practice, patient, null)
                                             .Select(this.GetSessionData)
                                             .ToList();

            return sessions;
        }

        private SessionData GetSessionData(SessionViewModel arg)
        {
            var anamneseGetter = this.ViewModelGetter<Anamnese, AnamneseViewModel>(AnamnesesController.GetViewModel);
            var receiptGetter = this.ViewModelGetter<Receipt, ReceiptViewModel>(ReceiptsController.GetViewModel);
            var physicalExaminationGetter = this.ViewModelGetter<PhysicalExamination, PhysicalExaminationViewModel>(PhysicalExaminationController.GetViewModel);
            var diagnosticHypothesisGetter = this.ViewModelGetter<DiagnosticHypothesis, DiagnosticHypothesisViewModel>(DiagnosticHypothesesController.GetViewModel);
            var examsGetter = this.ViewModelGetter<ExaminationRequest, ExaminationRequestViewModel>(ExamsController.GetViewModel);
            var examResultsGetter = this.ViewModelGetter<ExaminationResult, ExaminationResultViewModel>(ExamResultsController.GetViewModel);
            var diagnosisGetter = this.ViewModelGetter<Diagnosis, DiagnosisViewModel>(DiagnosisController.GetViewModel);
            var medicalCertificateGetter = this.ViewModelGetter<MedicalCertificate, MedicalCertificateViewModel>(MedicalCertificatesController.GetViewModel);

            var result = new SessionData
                {
                    Date = arg.Date,
                    Anamneses = this.db.Anamnese.Where(x => arg.AnamneseIds.Contains(x.Id))
                        .Select(anamneseGetter).ToList(),
                    Prescriptions = this.db.Receipts.Where(x => arg.ReceiptIds.Contains(x.Id))
                        .Select(receiptGetter).ToList(),
                    PhysicalExaminations = this.db.PhysicalExaminations.Where(x => arg.PhysicalExaminationIds.Contains(x.Id))
                        .Select(physicalExaminationGetter).ToList(),
                    DiagnosticHipotheses = this.db.DiagnosticHypotheses.Where(x => arg.DiagnosticHipothesesId.Contains(x.Id))
                        .Select(diagnosticHypothesisGetter).ToList(),
                    ExaminationRequests = this.db.ExaminationRequests.Where(x => arg.ExaminationRequestIds.Contains(x.Id))
                        .Select(examsGetter).ToList(),
                    ExaminationResults = this.db.ExaminationResults.Where(x => arg.ExaminationResultIds.Contains(x.Id))
                        .Select(examResultsGetter).ToList(),
                    Diagnosis = this.db.Diagnoses.Where(x => arg.DiagnosisIds.Contains(x.Id))
                        .Select(diagnosisGetter).ToList(),
                    MedicalCertificates = this.db.MedicalCertificates.Where(x => arg.MedicalCertificateIds.Contains(x.Id))
                        .Select(medicalCertificateGetter).ToList()
                };

            return result;
        }

        private Func<TModel, TViewModel> ViewModelGetter<TModel, TViewModel>(Func<TModel, Func<DateTime, DateTime>, TViewModel> fnc)
        {
            return m => fnc(m, this.ToLocalConverter());
        }

        private readonly object locker = new object();
        private Func<DateTime, DateTime> toLocalCache;
        private Func<DateTime, DateTime> ToLocalConverter()
        {
            if (this.toLocalCache == null)
                lock (this.locker)
                    if (this.toLocalCache == null)
                    {
                        var practiceId = this.practice.Id;
                        var timeZoneId = this.db.Practices.Where(p => p.Id == practiceId).Select(p => p.WindowsTimeZoneId).Single();
                        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                        Func<DateTime, DateTime> toLocal = utcDateTime => TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);

                        Thread.MemoryBarrier();

                        this.toLocalCache = toLocal;
                    }

            return this.toLocalCache;
        }

        public class PdfDoctorData : XmlDoctorData
        {
            public new string Gender
            {
                get { return EnumHelper.GetValueDisplayDictionary(typeof(TypeGender))[(int)base.Gender]; }
            }

            public new string MaritalStatus
            {
                get
                {
                    if (base.MaritalStatus.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeMaritalStatus))[(int)base.MaritalStatus.Value];
                    return null;
                }
            }

            public new string CpfOwner
            {
                get
                {
                    if (base.CpfOwner.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeCpfOwner))[(int)base.CpfOwner.Value];
                    return null;
                }
            }

            public new string MedicalEntityJurisdiction
            {
                get
                {
                    if (base.MedicalEntityJurisdiction.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeEstadoBrasileiro))[(int)base.MedicalEntityJurisdiction.Value];
                    return null;
                }
            }

            public new List<PdfPatientData> Patients
            {
                get { return base.Patients.OfType<PdfPatientData>().ToList(); }
            }
        }

        public class PdfPatientData : XmlPatientData
        {
            [Display(Name = "Sexo")]
            public new string Gender
            {
                get { return EnumHelper.GetValueDisplayDictionary(typeof(TypeGender))[(int)base.Gender]; }
            }

            [Display(Name = "Estado civil")]
            public new string MaritalStatus
            {
                get
                {
                    if (base.MaritalStatus.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeMaritalStatus))[(int)base.MaritalStatus.Value];
                    return null;
                }
            }

            [Display(Name = "Proprietário do CPF")]
            public new string CpfOwner
            {
                get
                {
                    if (base.CpfOwner.HasValue)
                        return EnumHelper.GetValueDisplayDictionary(typeof(TypeCpfOwner))[(int)base.CpfOwner.Value];
                    return null;
                }
            }
        }


        [XmlRoot("Doctor", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Doctor")]
        public class XmlDoctorData : UserViewModel
        {
            public new TypeGender Gender
            {
                get { return (TypeGender)base.Gender; }
                set { base.Gender = (int)value; }
            }

            public new TypeMaritalStatus? MaritalStatus
            {
                get { return (TypeMaritalStatus?)base.MaritalStatus; }
                set { base.MaritalStatus = (short?)value; }
            }

            public new TypeCpfOwner? CpfOwner
            {
                get { return (TypeCpfOwner?)base.CpfOwner; }
                set { base.CpfOwner = (short?)value; }
            }

            public new TypeEstadoBrasileiro? MedicalEntityJurisdiction
            {
                get { return (TypeEstadoBrasileiro?)base.MedicalEntityJurisdiction; }
                set { base.MedicalEntityJurisdiction = (int?)value; }
            }

            public List<XmlPatientData> Patients { get; set; }
        }

        [XmlRoot("Patient", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Patient")]
        public class XmlPatientData : PatientViewModel
        {
            public new TypeGender Gender
            {
                get { return (TypeGender)base.Gender; }
                set { base.Gender = (int)value; }
            }

            public new TypeMaritalStatus? MaritalStatus
            {
                get { return (TypeMaritalStatus?)base.MaritalStatus; }
                set { base.MaritalStatus = (short?)value; }
            }

            public new TypeCpfOwner? CpfOwner
            {
                get { return (TypeCpfOwner?)base.CpfOwner; }
                set { base.CpfOwner = (short?)value; }
            }

            public List<AppointmentViewModel> Appointments { get; set; }

            [XmlElement]
            public List<SessionData> Sessions { get; set; }
        }


        [XmlRoot("Session", Namespace = "http://www.cerebello.com.br", IsNullable = false)]
        [XmlType("Session")]
        public class SessionData
        {
            public DateTime Date { get; set; }

            public List<AnamneseViewModel> Anamneses { get; set; }
            public List<PhysicalExaminationViewModel> PhysicalExaminations { get; set; }
            public List<ReceiptViewModel> Prescriptions { get; set; }
            public List<ExaminationRequestViewModel> ExaminationRequests { get; set; }
            public List<ExaminationResultViewModel> ExaminationResults { get; set; }
            public List<DiagnosisViewModel> Diagnosis { get; set; }
            public List<MedicalCertificateViewModel> MedicalCertificates { get; set; }
            public List<DiagnosticHypothesisViewModel> DiagnosticHipotheses { get; set; }
        }
    }
}

