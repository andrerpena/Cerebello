using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controls;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class AppController : DoctorController
    {
        /// <summary>
        /// This class exists for the sole reason of allowing
        /// a lot of unios to be made consistently
        /// </summary>
        public class GlobalSearchIntermediateResult
        {
            /// <summary>
            /// Maybe "patient", "medication" and so forth
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Description of the type.
            /// Maybe "Paciente", "Medicamento" (something human descriptive)
            /// </summary>
            public string TypeDescription { get; set; }

            /// <summary>
            /// The id of the object
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The text that will be presented in the view
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// The relevance for this record 
            /// As we'll need to concatenate a lot of these because of the UNIONS,
            /// we need to make sure more relavant results come in front
            /// The lower the highest
            /// </summary>
            public int Relevance { get; set; }
        }

        /// <summary>
        /// Searches everything (well.. almost :))
        /// </summary>
        /// <remarks>
        /// Requirements:
        ///     1 - Should be able to search patients
        ///     2 - Should be able to search medications
        /// </remarks>
        [HttpGet]
        public JsonResult LookupEverything(string term, int pageSize, int pageIndex, int doctorId)
        {
            // We're gonna add a lot of queries that should result in an UNION statement

            var queries = new List<IQueryable<GlobalSearchIntermediateResult>>();

            var patientsQuery = this.db.Patients.Where(p => p.DoctorId == this.Doctor.Id);
            if (!string.IsNullOrEmpty(term))
                patientsQuery = patientsQuery.Where(p => p.Person.FullName.Contains(term));

            queries.Add(patientsQuery.Select(p =>
                        new GlobalSearchIntermediateResult()
                        {
                            Id = p.Id,
                            Text = p.Person.FullName,
                            Type = "patient",
                            TypeDescription = "Paciente",
                            Relevance = 1
                        }));

            // somente os médicos podem pesquisar por algumas informações
            if (this.DbUser.Doctor != null || this.DbUser.Administrator != null)
            {
                var medicinesQuery = this.db.Medicines.Where(p => p.DoctorId == this.Doctor.Id);
                if (!string.IsNullOrEmpty(term))
                    medicinesQuery = medicinesQuery.Where(p => p.Name.Contains(term));

                queries.Add(
                    medicinesQuery.Select(
                        m =>
                        new GlobalSearchIntermediateResult()
                            {
                                Id = m.Id,
                                Text = m.Name,
                                Type = "medicine",
                                TypeDescription = "Medicamento",
                                Relevance = 2
                            }));

                var laboratoriesQuery = this.db.Laboratories.Where(p => p.DoctorId == this.Doctor.Id);
                if (!string.IsNullOrEmpty(term))
                    laboratoriesQuery = laboratoriesQuery.Where(p => p.Name.Contains(term));

                queries.Add(
                    laboratoriesQuery.Select(
                        l =>
                        new GlobalSearchIntermediateResult()
                            {
                                Id = l.Id,
                                Text = l.Name,
                                Type = "laboratory",
                                TypeDescription = "Laboratório",
                                Relevance = 3
                            }));
            }

            var finalQuery = queries[0];

            for (var i = 1; i < queries.Count; i++)
                finalQuery = finalQuery.Union(queries[i]);

            // returns the URL given a type ("patient", "medicine"...) and an Id
            Func<string, int, string> getUrl = (type, id) =>
                {
                    switch (type)
                    {
                        case "patient":
                            return Url.Action("details", "patients", new { id });
                        case "medicine":
                            return Url.Action("details", "medicines", new { id });
                        case "laboratory":
                            return Url.Action("details", "laboratories", new { id });
                        default:
                            throw new Exception("Invalid type");
                    }
                };

            var result = (from r in finalQuery.OrderBy(r => r.Relevance).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                          select new GlobalSearchViewModel()
                          {
                              Id = r.Id,
                              Url = getUrl(r.Type, r.Id),
                              Value = r.Text,
                              Description = r.TypeDescription
                          }).ToList();

            var resultCount = finalQuery.Count();

            return Json(new AutocompleteJsonResult()
            {
                Count = resultCount,
                Rows = new System.Collections.ArrayList(result)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
