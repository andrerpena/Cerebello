using System.Linq;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class MedicalSpecialtiesController : RootController
    {
        [HttpGet]
        public JsonResult LookupMedicalSpecialties(string term, int pageSize, int pageIndex)
        {
            using (var db = this.CreateNewCerebelloEntities())
            {
                var baseQuery = db.SYS_MedicalSpecialty.AsQueryable();
                if (!string.IsNullOrEmpty(term))
                    baseQuery = baseQuery.Where(mp => mp.Name.Contains(term) || mp.Code.Contains(term));

                baseQuery = baseQuery.Where(ms => ms.Code != null && ms.Code != "");
                baseQuery = baseQuery.Where(ms => ms.Name != null && ms.Name != "");

                var query = from mp in baseQuery
                            orderby mp.Name
                            select new MedicalSpecialtiesLookupGridModel
                            {
                                Id = mp.Id,
                                Code = mp.Code,
                                Name = mp.Name,
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
}
