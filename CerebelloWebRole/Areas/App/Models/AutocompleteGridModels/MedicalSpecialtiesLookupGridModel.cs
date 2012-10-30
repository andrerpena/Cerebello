using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// GridModel used for displaying medical specialty lookups.
    /// </summary>
    public class MedicalSpecialtiesLookupGridModel
    {
        /// <summary>
        /// Medical specialty id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Medical specialty code.
        /// </summary>
        [Display(Name = "Código")]
        public string Code { get; set; }

        /// <summary>
        /// Medical specialty name.
        /// </summary>
        [Display(Name = "Nome")]
        public string Name { get; set; }
    }
}