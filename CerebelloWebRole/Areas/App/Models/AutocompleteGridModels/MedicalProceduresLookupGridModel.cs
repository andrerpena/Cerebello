using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// GridModel used for displaying medical procedures lookups.
    /// </summary>
    public class MedicalProceduresLookupGridModel
    {
        /// <summary>
        /// Medical procedure id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Medical procedure code.
        /// </summary>
        [Display(Name = "Código")]
        public string Code { get; set; }

        /// <summary>
        /// Medical procedure name.
        /// </summary>
        [Display(Name = "Nome")]
        public string Name { get; set; }
    }
}