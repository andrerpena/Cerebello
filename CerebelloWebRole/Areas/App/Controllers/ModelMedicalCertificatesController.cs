using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Areas.App.Controllers
{
    /// <summary>
    /// Medical certificate controllers
    /// </summary>
    [SelfOrUserRolePermission(UserRoleFlags.Administrator)]
    public class ModelMedicalCertificatesController : DoctorController
    {
        public ActionResult Index()
        {
            var model = new ModelMedicalCertificatesIndexViewModel
                {
                    Objects = (from m in this.db.ModelMedicalCertificates
                                             .Where(m => m.Doctor.Id == this.Doctor.Id)
                                             .OrderBy(m => m.Name).Take(Constants.RECENTLY_REGISTERED_LIST_MAXSIZE).ToList()
                               select new ModelMedicalCertificateViewModel()
                                   {
                                       Id = m.Id,
                                       Name = m.Name,
                                       Text = m.Text
                                   }).ToList(),
                    Count = this.db.ModelMedicalCertificates.Count()
                };
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var certificateModel = this.db.ModelMedicalCertificates.Include("Fields").First(m => m.Id == id);
            var model = new ModelMedicalCertificateViewModel()
                             {
                                 Id = certificateModel.Id,
                                 Name = certificateModel.Name,
                                 Text = certificateModel.Text
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
            var viewModel = new ModelMedicalCertificateViewModel();

            if (id != null)
            {
                var medicine = this.db.ModelMedicalCertificates.First(m => m.Id == id);
                viewModel = new ModelMedicalCertificateViewModel()
                             {
                                 Id = medicine.Id,
                                 Name = medicine.Name,
                                 Text = medicine.Text
                             };
                // todo: page title should be set in the view, not in the controller
                ViewBag.Title = "Alterando modelo de atestado médico: " + viewModel.Name;
            }
            else
                // todo: page title should be set in the view, not in the controller
                ViewBag.Title = "Novo modelo de atestado médico";

            return View("Edit", viewModel);
        }

        /// <remarks>
        /// Requirements:
        /// 
        ///     -   Only alpha-numeric characters, minus and underscores are allowed for fields.
        ///         One ModelState error must be added for each noncomplying field
        ///         
        ///     -   Place holders must follow the pattern <%PROPERTY_NAME%>.
        ///         Cannot use spaces between the field name and the place holder markers <% and %>
        ///         One ModelState error must be added for each noncomplying placeholder
        ///         
        ///     -   All declared fields must be referenced in the text.
        ///         One ModelState error must be added for each noncomplying field
        ///         
        ///     -   All fields referenced in the text must have been declared, with the exception of the special field <%PACIENTE%>
        ///         One ModelState error must be added for each noncomplying field
        ///         
        ///     -   It has to be possible to add a certificate model with no fields
        ///      
        ///     -   It can't be possible to create fields with pre-defined name. The only pre-defined value today is "paciente"
        ///         One ModelState error must be added for each noncomplying field
        ///     
        /// </remarks>
        [HttpPost]
        public ActionResult Edit(ModelMedicalCertificateViewModel formModel)
        {
            if (this.ModelState.IsValid)
            {
                ModelMedicalCertificate certificateModel = null;

                if (formModel.Id != null)
                    certificateModel = this.db.ModelMedicalCertificates.First(m => m.Id == formModel.Id);
                else
                {
                    certificateModel = new ModelMedicalCertificate { PracticeId = this.DbUser.PracticeId, };
                    this.db.ModelMedicalCertificates.AddObject(certificateModel);
                }

                certificateModel.Name = formModel.Name;
                certificateModel.Text = formModel.Text;
                certificateModel.Doctor = this.Doctor;
                certificateModel.PracticeId = this.DbPractice.Id;

                var fieldsFoundInText =
                    Regex.Matches(formModel.Text, @"<%(.+?)%>", RegexOptions.IgnoreCase).Cast<Match>().Select(m => m.Groups[1].Value.Trim()).ToList();

                var harakiriQueue = new Queue<ModelMedicalCertificateField>();

                // delete fields found in the DB that don't have a matching field in text
                foreach (var dbField in certificateModel.Fields.Where(dbField => fieldsFoundInText.All(f => f != dbField.Name)))
                    harakiriQueue.Enqueue(dbField);
                while (harakiriQueue.Any())
                    this.db.ModelMedicalCertificateFields.DeleteObject(harakiriQueue.First());

                // add new fields to the DB
                foreach (var field in fieldsFoundInText.Where(field => certificateModel.Fields.All(f => f.Name != field)).Where(field => StringHelper.RemoveDiacritics(field).ToLower() != "paciente"))
                    certificateModel.Fields.Add(
                        new ModelMedicalCertificateField()
                            {
                                PracticeId = this.DbUser.PracticeId,
                                Name = field
                            });

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
                while (childrenQueue.Count > 0)
                {
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

        /// <summary>
        /// Model medical certificate autocomplete
        /// </summary>
        [HttpGet]
        public JsonResult AutocompleteModelMedicalCertificates(string term, int pageSize, int pageIndex)
        {
            IQueryable<ModelMedicalCertificate> baseQuery = this.db.ModelMedicalCertificates;
            baseQuery = baseQuery.Where(c => c.DoctorId == this.Doctor.Id);
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(c => c.Name.Contains(term));


            var queryResult = (from c in baseQuery.OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                               select new AutocompleteRow
                               {
                                   Id = c.Id.ToString(),
                                   Value = c.Name
                               }).ToList();

            var result = new AutocompleteJsonResult
            {
                Rows = new ArrayList(queryResult),
                Count = baseQuery.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FieldEditor(ModelMedicalCertificateFieldViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}
