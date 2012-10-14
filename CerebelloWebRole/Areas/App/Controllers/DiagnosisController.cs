using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class DiagnosisController : DoctorsController
    {
        private static DiagnosisViewModel GetViewModel(Diagnosis diagnosis)
        {
            return new DiagnosisViewModel()
            {
                Id = diagnosis.Id,
                PatientId = diagnosis.PatientId,
                Text = diagnosis.Observations,
                Cid10Code = diagnosis.Cid10Code,
                Cid10Name = diagnosis.Cid10Name
            };
        }

        [HttpGet]
        public ActionResult Create(int patientId, string newKey)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(DiagnosisViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        public ActionResult Details(int id)
        {
            var diagnosis = this.db.Diagnoses.First(a => a.Id == id);
            return this.View(GetViewModel(diagnosis));
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            DiagnosisViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel((from a in db.Diagnoses where a.Id == id select a).First());
            else
                viewModel = new DiagnosisViewModel()
                {
                    Id = null,
                    PatientId = patientId
                };

            return View("Edit", viewModel);
        }

        /// <summary>
        /// Requirements:
        ///    - If both the notes and the Cid10Code are null or empty, an error must be added to ModelState
        ///    - Must properly create a diagnosis for the given patient with the given information
        /// </summary>
        /// <param name="formModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(DiagnosisViewModel formModel)
        {
            if (string.IsNullOrEmpty(formModel.Text) && string.IsNullOrEmpty(formModel.Cid10Code))
                this.ModelState.AddModelError("", "É necessário preencher um diagnóstico CID-10 ou as notas");

            if (this.ModelState.IsValid)
            {
                Diagnosis diagnosis = null;
                if (formModel.Id == null)
                {
                    diagnosis = new Diagnosis()
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value
                    };
                    this.db.Diagnoses.AddObject(diagnosis);
                }
                else
                    diagnosis = this.db.Diagnoses.First(a => a.Id == formModel.Id);

                diagnosis.Observations = formModel.Text;
                diagnosis.Cid10Code = formModel.Cid10Code;
                diagnosis.Cid10Name = formModel.Cid10Name;
                db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return View("details", GetViewModel(diagnosis));
            }

            return View("edit", formModel);
        }

        /// <summary>
        /// 
        /// Requisitos:
        ///     
        ///     1 - The given diagnosis should stop existing after this action call. In case of success, it should return a JsonDeleteMessage
        ///         with success = true
        ///     
        ///     2 - In case the given diagnosis doesn't exist, it should return a JsonDeleteMessage with success = false and the text property
        ///         informing the reason of the failure
        /// 
        /// </summary>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var diagnosis = this.db.Diagnoses.First(m => m.Id == id);
                this.db.Diagnoses.DeleteObject(diagnosis);
                this.db.SaveChanges();
                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lookup of diagnoses
        /// </summary>
        /// <remarks>
        /// Requirements:
        ///     1   Should return a AutocompleteJsonResult serialized in Json containing a the Count of CID-10 entries found
        ///         and the list of entries themselves. Each entry has a Value and an Id, the Value is the text, the Id
        ///         is the Cid10 category or sub-category of the condition
        /// </remarks>
        [HttpGet]
        public JsonResult AutocompleteDiagnoses(string term, int pageSize, int pageIndex)
        {
            // read CID10.xml as an embedded resource
            XmlReaderSettings settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };
            var reader = XmlReader.Create(Server.MapPath(@"~\data\CID10.xml"), settings);
            var doc = XDocument.Load(reader);

            var result = AutocompleteHelper.GetData<CidAutocompleteGridModel>(term, pageSize, pageIndex,
                t =>
                from e in doc.Descendants()
                where e.Name == "nome" &&
                StringHelper.RemoveDiacritics(e.ToString()).ToLower().Contains(StringHelper.RemoveDiacritics(t.ToString()).ToLower()) &&
                (e.Parent.Attribute("codcat") != null || e.Parent.Attribute("codsubcat") != null)
                select new CidAutocompleteGridModel { Cid10Name = e.Value, Cid10Code = e.Parent.Attribute("codcat") != null ? e.Parent.Attribute("codcat").Value : e.Parent.Attribute("codsubcat").Value });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}