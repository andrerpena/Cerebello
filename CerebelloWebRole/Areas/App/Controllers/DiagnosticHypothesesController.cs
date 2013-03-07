﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class DiagnosticHypothesesController : DoctorController
    {
        public static DiagnosticHypothesisViewModel GetViewModel(DiagnosticHypothesis diagnosticHypothesis)
        {
            return new DiagnosticHypothesisViewModel()
            {
                Id = diagnosticHypothesis.Id,
                Cid10Code = diagnosticHypothesis.Cid10Code,
                Cid10Name = diagnosticHypothesis.Cid10Name,
                Text = diagnosticHypothesis.Observations,
                PatientId = diagnosticHypothesis.PatientId
            };
        }

        public ActionResult Details(int id)
        {
            var diagnosticHypothesis = this.db.DiagnosticHypotheses.First(dh => dh.Id == id);
            return this.View(GetViewModel(diagnosticHypothesis));
        }

        [HttpGet]
        public ActionResult Create(int patientId)
        {
            return this.Edit(null, patientId);
        }

        [HttpPost]
        public ActionResult Create(DiagnosticHypothesisViewModel viewModel)
        {
            return this.Edit(viewModel);
        }


        [HttpGet]
        public ActionResult Edit(int? id, int? patientId)
        {
            DiagnosticHypothesisViewModel viewModel = null;

            if (id != null)
                viewModel = GetViewModel((from a in db.DiagnosticHypotheses where a.Id == id select a).First());
            else
                viewModel = new DiagnosticHypothesisViewModel()
                {
                    Id = null,
                    PatientId = patientId
                };

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(DiagnosticHypothesisViewModel formModel)
        {
            Debug.Assert(formModel.PatientId != null, "formModel.PatientId != null");
            if (this.ModelState.IsValid)
            {
                DiagnosticHypothesis diagnosticHypothesis = null;
                if (formModel.Id == null)
                {
                    diagnosticHypothesis = new DiagnosticHypothesis
                    {
                        CreatedOn = this.GetUtcNow(),
                        PatientId = formModel.PatientId.Value,
                        PracticeId = this.DbUser.PracticeId
                    };
                    this.db.DiagnosticHypotheses.AddObject(diagnosticHypothesis);
                }
                else
                    diagnosticHypothesis = this.db.DiagnosticHypotheses.First(a => a.Id == formModel.Id);

                diagnosticHypothesis.Observations = formModel.Text;
                diagnosticHypothesis.Cid10Code = formModel.Cid10Code;
                diagnosticHypothesis.Cid10Name = formModel.Cid10Name;

                db.SaveChanges();

                // todo: this shoud be a redirect... so that if user press F5 in browser, the object will no be saved again.
                return View("Details", GetViewModel(diagnosticHypothesis));
            }

            return View("Edit", formModel);
        }

        [HttpGet]
        public JsonResult AutocompleteDiagnoses(string term, int pageSize, int pageIndex)
        {
            IQueryable<SYS_Cid10> baseQuery = this.db.SYS_Cid10;
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(c => c.Name.Contains(term) || c.Cat.Contains(term) || c.SubCat.Contains(term));

            var query = from c in baseQuery
                        orderby c.Name
                        select new // CidAutocompleteGridModel
                        {
                            Cid10Code = c.Cat ?? c.SubCat,
                            Cid10Name = c.Name
                        };

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()),
                Count = query.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            try
            {
                var diagnosticHypothesis = this.db.DiagnosticHypotheses.First(m => m.Id == id);

                this.db.DiagnosticHypotheses.DeleteObject(diagnosticHypothesis);
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