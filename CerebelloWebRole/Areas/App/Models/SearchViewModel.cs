using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class SearchViewModel<TModel>
    {
        /// <summary>
        /// The search model
        /// </summary>
        public SearchModel SearchModel { get; set; }

        /// <summary>
        /// Objects (considering pagination. The total ammount here must be no more than the page size)
        /// </summary>
        public List<TModel> Objects { get; set; }

        /// <summary>
        /// Total of objects found (desregarding pagination)
        /// </summary>
        public int Count { get; set; }
    }
}