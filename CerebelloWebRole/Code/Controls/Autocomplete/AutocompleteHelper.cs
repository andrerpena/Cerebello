using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CerebelloWebRole.Code.Controls
{
    /// <summary>
    /// Ajuda a realizar operações comuns do Lookup
    /// </summary>
    public static class AutocompleteHelper
    {
        [Obsolete("This is to hard to generalize the autocomplete logic. This is just making stuff harder to understand. Just do it manually")]
        public static AutocompleteJsonResult GetData<TModel>(string term, int pageSize, int? pageIndex, Func<string, IEnumerable<TModel>> getQuery)
        {
            if (!pageIndex.HasValue)
                pageIndex = 1;

            if (term == null)
                term = string.Empty;

            if (getQuery == null) throw new ArgumentNullException("getQuery");

            var query = getQuery(term);
            var totalCount = query.Count();
            var pagedResult = query.Skip(pageSize * (pageIndex.Value - 1)).Take(pageSize).ToList();

            return new AutocompleteJsonResult()
            {
                Count = totalCount,
                Rows = new System.Collections.ArrayList((from r in pagedResult
                                                         select r).ToList())
            };
        }
    }
}
