using System.Linq;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class MedicalProceduresController : PracticeController
    {
        [HttpGet]
        public JsonResult LookupMedicalProcedures(string term, int pageSize, int pageIndex)
        {
            var baseQuery = this.db.SYS_MedicalProcedure.AsQueryable();
            if (!string.IsNullOrEmpty(term))
                baseQuery = baseQuery.Where(mp => mp.Name.Contains(term) || mp.Code.Contains(term));

            var query = from mp in baseQuery
                        orderby mp.Name
                        select new MedicalProceduresLookupGridModel
                        {
                            Id = mp.Id,
                            Name = mp.Name,
                            Code = mp.Code,
                        };

            var result = new AutocompleteJsonResult()
            {
                Rows = new System.Collections.ArrayList(query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()),
                Count = query.Count()
            };

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}