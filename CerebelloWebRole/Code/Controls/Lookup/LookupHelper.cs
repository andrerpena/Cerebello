using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CerebelloWebRole.Code.Controls
{
    /// <summary>
    /// Ajuda a realizar operações comuns do Lookup
    /// </summary>
    public class LookupHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public static TModel GetObject<TModel>(int? id, string text, Func<int, TModel> getById, Func<string, TModel> getByText, Func<string, TModel> getNew, bool allowInsertion) where TModel : class
        {
            if (allowInsertion)
            {
                if (string.IsNullOrEmpty(text))
                    return null;

                if (id.HasValue)
                    return getById(id.Value);

                var @object = getByText(text);
                if (@object == null)
                    @object = getNew(text);

                return @object;
            }
            else
            {
                if (id.HasValue)
                    return getById(id.Value);

                return null;
            }
        }

        public static LookupJsonResult GetData<TModel>(string term, int pageSize, int? pageIndex, Func<string, IQueryable<TModel>> getQuery, Func<IQueryable<TModel>, IQueryable<TModel>> orderQueryBy, Func<TModel, object> createRow)
        {
            if (!pageIndex.HasValue)
                pageIndex = 1;

            if (term == null)
                term = string.Empty;

            var query = getQuery(term);
            var totalCount = query.Count();
            var pagedResult = orderQueryBy(query).Skip(pageSize * (pageIndex.Value - 1)).Take(pageSize).ToList();

            return new LookupJsonResult()
                {
                    Count = totalCount,
                    Rows = new System.Collections.ArrayList((from r in pagedResult
                                                             select createRow(r)).ToList())
                };
        }

        public static LookupJsonResult GetData<TModel>(string term, int pageSize, int? pageIndex, Func<string, IEnumerable<TModel>> getQuery)
        {
            if (!pageIndex.HasValue)
                pageIndex = 1;

            if (term == null)
                term = string.Empty;

            var query = getQuery(term);
            var totalCount = query.Count();
            var pagedResult = query.Skip(pageSize * (pageIndex.Value - 1)).Take(pageSize).ToList();

            return new LookupJsonResult()
            {
                Count = totalCount,
                Rows = new System.Collections.ArrayList((from r in pagedResult
                                                         select r).ToList())
            };
        }
    }
}
