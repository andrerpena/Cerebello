using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public ActionResult Create(MedicalCertificateViewModel formModel)
        {
            return this.Edit(formModel);
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
                MedicalCertificate certificate = db.MedicalCertificates.Where(mc => mc.Id == id.Value).FirstOrDefault();
                viewModel = new MedicalCertificateViewModel()
                {
                    Id = certificate.Id,
                    PatientId = certificate.PatientId,
                    ModelId = certificate.ModelMedicalCertificateId,
                    Fields = (from f in certificate.Fields
                              select new MedicalCertificateFieldViewModel()
                              {
                                  Id = f.Id,
                                  Name = f.Name,
                                  Value = f.Value
                              }).ToList()
                };
            }
            else
            {
                viewModel = new MedicalCertificateViewModel()
                {
                    Id = id,
                    PatientId = patientId
                };
            }

            // NOTE: The ModelMedicalCertificateController is responsible for keeping medical certificate
            // fields up to date with the model
            // At this point, viewModel.Fields must be fully populated
            string selectedModelValue = viewModel.ModelId.HasValue ? viewModel.ModelId.ToString() : null;
            viewModel.ModelOptions = this.db.ModelMedicalCertificates
                .ToList()
                .Select(mmc => new SelectListItem() { Text = mmc.Name, Value = mmc.Id.ToString() })
                .ToList();

            return View("Edit", viewModel);
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
        public ActionResult Edit(MedicalCertificateViewModel formModel)
        {
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
                    foreach (var field in certificateModel.Fields)
                    {
                        if (!formModel.Fields.Any(f => f.Name.ToLower() == field.Name.ToLower()))
                        {
                            this.ModelState.AddModelError<MedicalCertificateViewModel>(m => m.Fields, "Dados inválidos. As informações recebidas não condizem com o modelo de atestado especificado");
                            break;
                        }
                    }

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

            MedicalCertificate certificate = null;

            if (this.ModelState.IsValid)
            {
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
                    certificate = db.MedicalCertificates.Where(r => r.Id == formModel.Id).FirstOrDefault();
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
                        },
                        (m) => { this.db.MedicalCertificateFields.DeleteObject(m); }
                    );

                this.db.SaveChanges();

                var viewModel = new MedicalCertificateViewModel()
                {
                    Id = certificate.Id,
                    ModelId = certificate.ModelMedicalCertificateId,
                    // the only situation in which ModelName will be null is when the model certificate has been removed
                    ModelName = certificate.ModelMedicalCertificate != null ? certificate.ModelMedicalCertificate.Name : null,
                    PatientId = certificate.PatientId,
                    Fields = (from f in certificate.Fields
                              select new MedicalCertificateFieldViewModel()
                              {
                                  Id = f.Id,
                                  Name = f.Name,
                                  Value = f.Value
                              }).ToList()
                };

                return this.View("Details", viewModel);
            }

            formModel.ModelOptions = this.db.ModelMedicalCertificates.ToList().Select(mmc => new SelectListItem() { Text = mmc.Name, Value = mmc.Id.ToString() }).ToList();
            return View("Edit", formModel);
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
        public ActionResult MedicalCertificateFieldsEditor(int? modelId, int? certificateId)
        {
            var viewModel = new MedicalCertificateViewModel() { Fields = new List<MedicalCertificateFieldViewModel>() };
            var model = this.db.ModelMedicalCertificates.Where(mmc => mmc.Id == modelId).FirstOrDefault();

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

            return this.View(viewModel);
        }

        /// <summary>
        /// Returns the text for the certificate. The text that must be placed on the PDF
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetCertificateText(int id)
        {
            var certificate = this.db.MedicalCertificates.Where(mmc => mmc.Id == id).FirstOrDefault();
            if (certificate == null)
                throw new Exception("Couldn't find certificate. Certificate id: " + id);

            var certificateText = certificate.Text;
            foreach (var field in certificate.Fields)
                certificateText = StringHelper.ReplaceString(certificateText, "<%" + field.Name + "%>", field.Value, StringComparison.OrdinalIgnoreCase);

            // the special case of patient
            certificateText = StringHelper.ReplaceString(certificateText, "<%paciente%>", certificate.Patient.Person.FullName, StringComparison.OrdinalIgnoreCase);

            return certificateText;
        }

        public ActionResult ViewPDF(int id)
        {
            var documentSize = PageSize.A4;
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
