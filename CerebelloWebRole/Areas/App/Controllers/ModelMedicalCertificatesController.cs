using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Controllers;
using Cerebello.Model;
using System.Globalization;
using System.Text.RegularExpressions;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Medical certificate controllers
    /// </summary>
    public class ModelMedicalCertificatesController : DoctorController
    {
        public ActionResult Index()
        {
            var model = new ModelMedicalCertificatesIndexViewModel();
            model.Objects = (from m in this.db.ModelMedicalCertificates.Where(m => m.Doctor.Id == this.Doctor.Id).OrderBy(m => m.Name).Take(5).ToList()
                             select new ModelMedicalCertificateViewModel()
                             {
                                 Id = m.Id,
                                 Name = m.Name,
                                 Text = m.Text
                             }).ToList();
            model.Count = db.Medicines.Count();
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var certificateModel = db.ModelMedicalCertificates.Include("Fields").Where(m => m.Id == id).First();
            var model = new ModelMedicalCertificateViewModel()
                             {
                                 Id = certificateModel.Id,
                                 Name = certificateModel.Name,
                                 Text = certificateModel.Text,
                                 Fields = certificateModel.Fields.Select(f => new ModelMedicalCertificateFieldViewModel() { Id = f.Id, Name = f.Name }).ToList()
                             };

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return this.Edit((int?)null);
        }

        [HttpPost]
        public ActionResult Create(ModelMedicalCertificateViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? anvisaId = null)
        {
            ModelMedicalCertificateViewModel viewModel = new ModelMedicalCertificateViewModel();

            if (id != null)
            {
                var medicine = db.ModelMedicalCertificates.Include("Fields").Where(m => m.Id == id).First();
                viewModel = new ModelMedicalCertificateViewModel()
                             {
                                 Id = medicine.Id,
                                 Name = medicine.Name,
                                 Text = medicine.Text,
                                 Fields = medicine.Fields.Select(f => new ModelMedicalCertificateFieldViewModel() { Id = f.Id, Name = f.Name }).ToList()
                             };
                ViewBag.Title = "Alterando modelo de atestado médico: " + viewModel.Name;
            }
            else
                ViewBag.Title = "Novo modelo de atestado médico";

            return View("Edit", viewModel);
        }

        /// <remarks>
        /// Requirements:
        /// 
        ///     1   Only alpha-numeric characters, minus and underscores are allowed for fields.
        ///         One ModelState error must be added for each noncomplying field
        ///         
        ///     2   Place holders must follow the pattern <%PROPERTY_NAME%>.
        ///         Cannot use spaces between the field name and the place holder markers <% and %>
        ///         One ModelState error must be added for each noncomplying placeholder
        ///         
        ///     3   All declared fields must be referenced in the text.
        ///         One ModelState error must be added for each noncomplying field
        ///         
        ///     4   All fields referenced in the text must have been declared, with the exception of the special field <%PACIENTE%>
        ///         One ModelState error must be added for each noncomplying field
        ///         
        ///     5   It has to be possible to add a certificate model with no fields
        ///     
        /// </remarks>
        [HttpPost]
        public ActionResult Edit(ModelMedicalCertificateViewModel formModel)
        {
            // validate that fields all have only valid characters
            for (var i = 0; i < formModel.Fields.Count; i++)
            {
                var field = formModel.Fields[i];

                // this will generate a decomposed form of the given string, with accents placed in different characters
                var stStr = field.Name.Normalize(System.Text.NormalizationForm.FormD);
                foreach (var c in stStr)
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (uc == UnicodeCategory.NonSpacingMark)
                    {
                        this.ModelState.AddModelError(string.Format("Fields[{0}].Name", i), "O formato de um ou mais campos é inválido");
                        break;
                    }

                    if (!char.IsLetterOrDigit(c) && !new char[] { '_', '-' }.Contains(c))
                    {
                        this.ModelState.AddModelError(string.Format("Fields[{0}].Name", i), "O formato de um ou mais campos é inválido");
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(formModel.Text))
            {
                // validate that fields are correctly formed
                var matchingProblems = Regex.Matches(formModel.Text, @"<%\s+([0-9a-z_-]+)%>", RegexOptions.IgnoreCase);
                foreach (var problem in matchingProblems)
                    this.ModelState.AddModelError<ModelMedicalCertificateViewModel>(m => m.Text, "Referências para campos não podem começar com espaços");

                matchingProblems = Regex.Matches(formModel.Text, @"<%([0-9a-z_-]+)\s+%>", RegexOptions.IgnoreCase);
                foreach (var problem in matchingProblems)
                    this.ModelState.AddModelError<ModelMedicalCertificateViewModel>(m => m.Text, "Referências para campos não podem terminar com espaços");

                matchingProblems = Regex.Matches(formModel.Text, @"<%\s*%>", RegexOptions.IgnoreCase);
                foreach (var problem in matchingProblems)
                    this.ModelState.AddModelError<ModelMedicalCertificateViewModel>(m => m.Text, "Referências para campos não podem ser vazias");

                // this will be used to see if every declared field is referenced in the text
                var fieldsFoundInTheText = new List<string>();

                // validate that all fields in the text are referenced
                foreach (Match match in Regex.Matches(formModel.Text, "<%([0-9a-z_-]+)%>", RegexOptions.IgnoreCase))
                {
                    var field = match.Groups[1].Value;
                    if (field.ToLower() != "paciente")
                    {
                        fieldsFoundInTheText.Add(match.Groups[1].Value);
                        if (!formModel.Fields.Any(f => f.Name.ToLower() == match.Groups[1].Value.ToLower()))
                            this.ModelState.AddModelError<ModelMedicalCertificateViewModel>(m => m.Text, string.Format("Uma referência foi feita para o campo '{0}' mas este não foi definido", match.Groups[1].Value));
                    }
                }

                // validate that all fields declared are in the text are declared
                for (int i = 0; i < formModel.Fields.Count; i++)
                {
                    var field = formModel.Fields[i];
                    if (!fieldsFoundInTheText.Any(f => f.ToLower() == field.Name.ToLower()))
                        this.ModelState.AddModelError(string.Format("Fields[{0}].Name", i), string.Format("O campo '{0}' não foi referenciado no texto", field.Name));
                }
            }


            if (this.ModelState.IsValid)
            {
                ModelMedicalCertificate certificateModel = null;

                if (formModel.Id != null)
                    certificateModel = db.ModelMedicalCertificates.Where(m => m.Id == formModel.Id).First();
                else
                {
                    certificateModel = new ModelMedicalCertificate();
                    this.db.ModelMedicalCertificates.AddObject(certificateModel);
                }

                certificateModel.Name = formModel.Name;
                certificateModel.Text = formModel.Text;
                certificateModel.Doctor = this.Doctor;

                certificateModel.Fields.Update(
                    formModel.Fields,
                    (vm, m) => vm.Id == m.Id,
                    (vm, m) =>
                    {
                        m.Name = vm.Name;
                    },
                    (m) =>
                    {
                        this.db.ModelMedicalCertificateFields.DeleteObject(m);
                    });

                // it's needed to synchronize certificates that use this model to make sure that they are up to date
                // it's possible that this can be optimized
                foreach (var certificate in certificateModel.MedicalCertificates)
                {
                    // find the differences between lists
                    var mustBeAdded = new Queue<ModelMedicalCertificateField>(certificateModel.Fields.Where(cm => !certificate.Fields.Any(c => c.Name.ToLower() == cm.Name.ToLower())).ToList());
                    var mustBeDeleted = new Queue<MedicalCertificateField>(certificate.Fields.Where(c => !certificateModel.Fields.Any(cm => cm.Name.ToLower() == c.Name.ToLower())).ToList());

                    while (mustBeAdded.Count > 0)
                    {
                        var toBeAdded = mustBeAdded.Dequeue();
                        certificate.Fields.Add(new MedicalCertificateField()
                        {
                            Name = toBeAdded.Name
                        });
                    }

                    while (mustBeDeleted.Count > 0)
                    {
                        var toBeDeleted = mustBeDeleted.Dequeue();
                        this.db.MedicalCertificateFields.DeleteObject(toBeDeleted);
                    }
                }

                db.SaveChanges();

                return Redirect(Url.Action("details", new { id = certificateModel.Id }));
            }

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
                var certificateModel = this.db.ModelMedicalCertificates.First(m => m.Id == id);

                var childrenQueue = new Queue<ModelMedicalCertificateField>(certificateModel.Fields);
                while(childrenQueue.Count > 0) {
                    var child = childrenQueue.Dequeue();
                    this.db.ModelMedicalCertificateFields.DeleteObject(child);
                }

                this.db.ModelMedicalCertificates.DeleteObject(certificateModel);
                this.db.SaveChanges();

                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult FieldEditor(ModelMedicalCertificateFieldViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}
