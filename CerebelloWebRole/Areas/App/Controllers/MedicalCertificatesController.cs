using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.iText;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class MedicalCertificatesController : DoctorController
    {
        public ActionResult Details(int id)
        {
            var model = this.db.MedicalCertificates.Include("ModelMedicalCertificate").FirstOrDefault(m => m.Id == id);
            var viewModel = GetViewModel(model);

            return View(viewModel);
        }

        public static MedicalCertificateViewModel GetViewModel(MedicalCertificate model)
        {
            var viewModel = new MedicalCertificateViewModel()
                                {
                                    Id = model.Id,
                                    PatientId = model.PatientId,
                                    ModelName = model.ModelMedicalCertificate != null ? model.ModelMedicalCertificate.Name : null,
                                    IssuanceDate = model.IssuanceDate,
                                    Fields = (from f in model.Fields
                                              select new MedicalCertificateFieldViewModel()
                                                         {
                                                             Id = f.Id,
                                                             Name = f.Name,
                                                             Value = f.Value
                                                         }).ToList()
                                };
            return viewModel;
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(MedicalCertificateViewModel[] medicalCertificates)
        {
            return this.Edit(medicalCertificates);
        }

        /// <summary>
        /// Edits a medical certificate
        /// </summary>
        /// <remarks>
        /// Requirements:
        /// 
        ///     1   If no ID is supplied, it's assumed to be new, that is, the user is going to select one now.
        ///     
        /// </remarks>
        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            MedicalCertificateViewModel viewModel = null;

            if (id.HasValue)
            {
                var certificate = this.db.MedicalCertificates.FirstOrDefault(mc => mc.Id == id.Value);
                // todo: use GetViewModel method
                //viewModel = GetViewModel(certificate);
                viewModel = new MedicalCertificateViewModel()
                {
                    Id = certificate.Id,
                    PatientId = certificate.PatientId,
                    ModelName = certificate.ModelMedicalCertificate.Name,
                    ModelId = certificate.ModelMedicalCertificateId,
                    IssuanceDate = certificate.IssuanceDate,
                    Fields = (from f in certificate.Fields
                              select new MedicalCertificateFieldViewModel
                              {
                                  Id = f.Id,
                                  Name = f.Name,
                                  Value = f.Value,
                              }).ToList(),
                };
            }
            else
            {
                viewModel = new MedicalCertificateViewModel()
                {
                    Id = id,
                    PatientId = patientId,
                    IssuanceDate = this.GetPracticeLocalNow(),
                };
            }

            return this.View("Edit", viewModel);
        }

        /// <remarks>
        /// Requirements:
        /// 
        ///     1   A model state error must be added if formModel.ModelId exists and does not reference a valid certificate model
        ///     
        ///     2   A model state error must be added if formModel.PatientId does not reference a valid patient
        ///     
        ///     3   A model state must be added if formModel.ModelId is null and formModel.Id is null as well
        /// 
        ///     4   If the model exists, for all fields in the certificate model, there must be a field in the formModel
        ///         One single model state error must be added if this requirement is not fulfilled
        ///         
        ///     5   If the model does not exist, all fields must be accepted
        ///     
        /// Observations:
        ///     
        ///     
        ///     
        /// </remarks>
        /// <param name="formModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(MedicalCertificateViewModel[] medicalCertificates)
        {
            var formModel = medicalCertificates[0];

            ModelMedicalCertificate certificateModel = null;

            // validates the existence and the compliance of the certificate model
            if (formModel.ModelId.HasValue)
            {
                certificateModel = this.db.ModelMedicalCertificates.FirstOrDefault(mmc => mmc.Id == formModel.ModelId);
                if (certificateModel == null)
                    this.ModelState.AddModelError<MedicalCertificateViewModel>(m => m.ModelId, "O modelo de atestado informado não é válido");
                else
                {
                    // for each field in the model, all must exist in the formModel
                    if (certificateModel.Fields.Any(field => formModel.Fields.All(f => f.Name.ToLower() != field.Name.ToLower())))
                        this.ModelState.AddModelError<MedicalCertificateViewModel>(m => m.Fields, "Dados inválidos. As informações recebidas não condizem com o modelo de atestado especificado");

                    // #KNOWN ISSUE# The next statements shouldn't exist. The REQUIRED attribute should work :(
                    // for all fields existing in the formModel, all must have a value
                    for (var i = 0; i < formModel.Fields.Count; i++)
                    {
                        var field = formModel.Fields[i];
                        if (string.IsNullOrEmpty(field.Value))
                            this.ModelState.AddModelError("Fields[" + i + "]", "O valor do campo é requerido");
                    }
                }
            }

            if (!formModel.ModelId.HasValue && !formModel.Id.HasValue)
                this.ModelState.AddModelError<MedicalCertificateViewModel>(m => m.ModelId, "É necessário informar o modelo do atestado");

            // validates the existence of the patient
            if (formModel.PatientId.HasValue && !this.db.Patients.Any(m => m.Id == formModel.PatientId))
                this.ModelState.AddModelError<MedicalCertificateViewModel>(m => m.ModelId, "O paciente informado não é válido");

            if (this.ModelState.IsValid)
            {
                MedicalCertificate certificate = null;
                if (formModel.Id == null)
                {
                    certificate = new MedicalCertificate()
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId,
                    };
                    this.db.MedicalCertificates.AddObject(certificate);
                }
                else
                {
                    certificate = this.db.MedicalCertificates.FirstOrDefault(r => r.Id == formModel.Id);
                    if (certificate == null)
                        return this.ObjectNotFound();
                }

                if (certificateModel != null)
                {
                    certificate.ModelMedicalCertificateId = certificateModel.Id;
                    certificate.Text = certificateModel.Text;
                }
                else
                    certificate.ModelMedicalCertificateId = null;

                certificate.Fields.Update(
                        formModel.Fields,
                        (vm, m) => vm.Name == m.Name,
                        (vm, m) =>
                        {
                            m.Name = vm.Name;
                            m.Value = vm.Value;
                            m.PracticeId = this.DbUser.PracticeId;
                        },
                        (m) => this.db.MedicalCertificateFields.DeleteObject(m));

                certificate.IssuanceDate = formModel.IssuanceDate.Value;
                this.db.SaveChanges();

                // todo: use GetViewModel method:
                //var viewModel = GetViewModel(certificate);

                var viewModel = new MedicalCertificateViewModel
                {
                    Id = certificate.Id,
                    ModelId = certificate.ModelMedicalCertificateId,
                    // the only situation in which ModelName will be null is when the model certificate has been removed
                    ModelName = certificate.ModelMedicalCertificate != null ? certificate.ModelMedicalCertificate.Name : null,
                    PatientId = certificate.PatientId,
                    IssuanceDate = certificate.IssuanceDate,
                    Fields = (from f in certificate.Fields
                              select new MedicalCertificateFieldViewModel()
                              {
                                  Id = f.Id,
                                  Name = f.Name,
                                  Value = f.Value,
                              }).ToList(),
                };

                return this.View("Details", viewModel);
            }

            formModel.ModelOptions = this.db.ModelMedicalCertificates.ToList().Select(mmc => new SelectListItem() { Text = mmc.Name, Value = mmc.Id.ToString() }).ToList();
            return this.View("Edit", formModel);
        }

        /// <remarks>
        /// Requirements:
        ///     
        ///     1   All fields must be delete as well
        ///     
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult Delete(int id)
        {
            try
            {
                var certificateModel = this.db.MedicalCertificates.First(m => m.Id == id);

                var childrenQueue = new Queue<MedicalCertificateField>(certificateModel.Fields);
                while (childrenQueue.Count > 0)
                {
                    var child = childrenQueue.Dequeue();
                    this.db.MedicalCertificateFields.DeleteObject(child);
                }

                this.db.MedicalCertificates.DeleteObject(certificateModel);
                this.db.SaveChanges();

                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns an editor view with the fields described by the given certificate model
        /// </summary>
        /// <remarks>
        /// Requirements:
        ///     1 - If no modelId is supplied, the certificateId is ignored and the default view must be returned
        ///         with a new MedicalCertificateViewModel as model
        ///     2 - If a modelId is supplied and a certificateId is not supplied, the default view must be returned
        ///         with a MedicalCertificateViewModel containing the fields of the specified model
        ///     3 - If a modelId is supplied and a certificateId is supplied and they don't match, the result must
        ///         be equal to case [2]
        ///     4 - If a modelID is supplied and a certificateId is supplied and they match, the default view must
        ///         be returned with a MedicalCertificateViewModel containing the fields of the specified model
        ///         but taking into account fields already registered for the given certificate
        /// </remarks>
        [HttpGet]
        public ActionResult MedicalCertificateFieldsEditor(int? modelId, int? certificateId, string htmlFieldPrefix)
        {
            var viewModel = new MedicalCertificateViewModel() { Fields = new List<MedicalCertificateFieldViewModel>() };
            var model = this.db.ModelMedicalCertificates.FirstOrDefault(mmc => mmc.Id == modelId);

            if (model != null)
            {
                foreach (var field in model.Fields)
                    viewModel.Fields.Add(new MedicalCertificateFieldViewModel()
                    {
                        Name = field.Name,
                        Value = null
                    });

                var certificate = this.db.MedicalCertificates.FirstOrDefault(mc => mc.Id == certificateId && mc.ModelMedicalCertificateId == modelId);
                if (certificate != null)
                {
                    foreach (var field in certificate.Fields)
                    {
                        var matchingField = viewModel.Fields.FirstOrDefault(f => f.Name == field.Name);
                        if (matchingField != null)
                            matchingField.Value = field.Value;
                    }
                }
            }

            this.ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            return this.View(viewModel);
        }

        /// <summary>
        /// Returns the text for the certificate. The text that must be placed on the PDF
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetCertificateText(int id)
        {
            var certificate = this.db.MedicalCertificates.FirstOrDefault(mmc => mmc.Id == id);
            if (certificate == null)
                throw new Exception("Couldn't find certificate. Certificate id: " + id);

            return Regex.Replace(
                certificate.Text,
                "<%(.+?)%>",
                match =>
                {
                    if (StringHelper.RemoveDiacritics(match.Groups[1].Value).ToLower() == "paciente")
                        return certificate.Patient.Person.FullName;
                    var matchingField = certificate.Fields.FirstOrDefault(f => f.Name.Trim() == match.Groups[1].Value.Trim());
                    if (matchingField != null)
                        return matchingField.Value;
                    throw new Exception("Não foi possível encontrar o valor de um campo. Campo: " + match.Groups[1].Value);
                });
        }

        public ActionResult ViewPdf(int id)
        {
            var document = new Document(PageSize.A4, 36, 36, 80, 80);
            var art = new Rectangle(50, 50, 545, 792);
            var documentStream = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, documentStream);
            writer.SetBoxSize("art", art);
            writer.PageEvent = new PdfHeaderFooter(this.Doctor.CFG_Documents);

            writer.CloseStream = false;

            document.Open();

            var certificateText = this.GetCertificateText(id);
            document.Add(new Paragraph(certificateText, new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, BaseColor.BLACK)));

            document.Close();
            documentStream.Position = 0;
            return File(documentStream, "application/pdf");
        }

    }
}
