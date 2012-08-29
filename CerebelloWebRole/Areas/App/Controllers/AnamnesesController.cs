using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AnamnesesController : DoctorController
    {
        private AnamneseViewModel GetViewModel(Anamnese anamnese)
        {
            return new AnamneseViewModel()
            {
                Id = anamnese.Id,
                PatientId = anamnese.PatientId,
                Text = anamnese.Text,
                Diagnoses = (from s in anamnese.Diagnoses
                             select new DiagnosisViewModel
                             {
                                 Text = s.Cid10Name,
                                 Cid10Code = s.Cid10Code

                             }).ToList()
            };
        }

        public ActionResult Details(int id)
        {
            var anamnese = db.Anamnese.Where(a => a.Id == id).First();
            return this.View(this.GetViewModel(anamnese));
        }

        [HttpGet]
        public ActionResult Create(int patientId)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(AnamneseViewModel viewModel)
        {
            return this.Edit(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            AnamneseViewModel viewModel = null;

            if (id != null)
                viewModel = this.GetViewModel((from a in db.Anamnese where a.Id == id select a).First());
            else
                viewModel = new AnamneseViewModel()
                {
                    Id = id,
                    PatientId = patientId
                };

            return View("Edit", viewModel);
        }

        /// <summary>
        /// Requirements:
        ///    - The list of diagnoses passed in must be sinchronized with the server
        /// </summary>
        /// <param name="formModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(AnamneseViewModel formModel)
        {
            Anamnese anamnese = null;

            if (this.ModelState.IsValid)
            {
                if (formModel.Id == null)
                {
                    anamnese = new Anamnese()
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value
                    };
                    this.db.Anamnese.AddObject(anamnese);
                }
                else
                    anamnese = db.Anamnese.Where(a => a.Id == formModel.Id).FirstOrDefault();

                anamnese.Text = formModel.Text;

                #region Update Diagnoses
                // step 1: add new
                foreach (var diagnosis in formModel.Diagnoses)
                {
                    if (!anamnese.Diagnoses.Any(ans => ans.Cid10Code == diagnosis.Cid10Code))
                        anamnese.Diagnoses.Add(new Diagnosis()
                        {
                            Cid10Code = diagnosis.Cid10Code,
                            Cid10Name = diagnosis.Text
                        });
                }

                Queue<Diagnosis> harakiriQueue = new Queue<Diagnosis>();

                // step 2: remove deleted
                foreach (var diagnosis in anamnese.Diagnoses)
                {
                    if (!formModel.Diagnoses.Any(ans => ans.Cid10Code == diagnosis.Cid10Code))
                        harakiriQueue.Enqueue(diagnosis);
                }

                while (harakiriQueue.Count > 0)
                    this.db.Diagnoses.DeleteObject(harakiriQueue.Dequeue());
                #endregion

                db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return View("details", this.GetViewModel(anamnese));
            }

            return View("edit", formModel);
        }

        /// <summary>
        /// Will retrieve an editor for diagnosis. This is useful for the collection editor to request a new editor for 
        /// a newly create diagnosis at client-side
        /// </summary>
        /// <param name="formModel"></param>
        /// <returns></returns>
        public ActionResult DiagnosisEditor(DiagnosisViewModel formModel)
        {
            return View(formModel);
        }

        /// <summary>
        /// Lookup of diagnoses
        /// </summary>
        /// <remarks>
        /// Requirements:
        ///     1   Should return a LookupJsonResult serialized in Json containing a the Count of CID-10 entries found
        ///         and the list of entries themselves. Each entry has a Value and an Id, the Value is the text, the Id
        ///         is the Cid10 category or sub-category of the condition
        /// </remarks>
        [HttpGet]
        public JsonResult LookupDiagnoses(string term, int pageSize, int pageIndex)
        {
            // read CID10.xml as an embedded resource
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(Server.MapPath(@"~\data\CID10.xml"), settings);
            XDocument doc = XDocument.Load(reader);

            var result = LookupHelper.GetData<CidLookupGridModel>(term, pageSize, pageIndex,
                t =>
                from e in doc.Descendants()
                where e.Name == "nome" &&
                StringHelper.RemoveDiacritics(e.ToString()).ToLower().Contains(StringHelper.RemoveDiacritics(t.ToString()).ToLower()) &&
                (e.Parent.Attribute("codcat") != null || e.Parent.Attribute("codsubcat") != null)
                select new CidLookupGridModel { Cid10Name = e.Value, Cid10Code = e.Parent.Attribute("codcat") != null ? e.Parent.Attribute("codcat").Value : e.Parent.Attribute("codsubcat").Value });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// Requisitos:
        ///     
        ///     1 - The given anamnese should stop existing after this action call. In case of success, it should return a JsonDeleteMessage
        ///         with success = true
        ///     
        ///     2 - In case the given anamnese doesn't exist, it should return a JsonDeleteMessage with success = false and the text property
        ///         informing the reason of the failure
        /// 
        /// </summary>
        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var anamnese = db.Anamnese.Where(m => m.Id == id).First();

                // get rid of associations
                while (anamnese.Diagnoses.Count > 0)
                    this.db.Diagnoses.DeleteObject(anamnese.Diagnoses.ElementAt(0));

                this.db.Anamnese.DeleteObject(anamnese);
                this.db.SaveChanges();
                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
