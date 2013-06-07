using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// GridModel used for displaying data within the CID autocomplete
    /// </summary>
    public class CidAutocompleteGridModel
    {
        /// <summary>
        /// Code CID10
        /// </summary>
        [Display(Name = "CID-10")]
        public string Cid10Code { get; set; }

        /// <summary>
        /// Name of the condition
        /// </summary>
        [Display(Name = "Descrição")]
        public string Cid10Name { get; set; }
    }
}