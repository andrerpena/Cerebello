using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class SearchModel
    {
        public SearchModel()
        {
            // pages must be 1-based
            this.Page = 1;
        }

        /// <summary>
        /// Search term
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Page index (this property must match the page grid's page parameter so do not rename it)
        /// </summary>
        public int Page { get; set; }


    }
}