using System.Collections;

namespace CerebelloWebRole.Code.Controls
{
    /// <summary>
    /// Representa o retorno JSON de uma chamada Ajax para popular o Lookup
    /// </summary>
    public class AutocompleteJsonResult
    {
        public AutocompleteJsonResult()
        {
            this.Rows = new ArrayList();
        }

        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the page index. 
        /// When the page parameter is missing from original request, 
        /// the first page for the search-term in the full dataset is returned.
        /// </summary>
        public int Page { get; set; }

        public ArrayList Rows { get; set; }
    }
}
