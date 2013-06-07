using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// GridModel used for displaying medicine autocomplete
    /// </summary>
    public class MedicineLookupGridModel
    {
        /// <summary>
        /// Medicine Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Medicine Name
        /// </summary>
        [Display(Name="Medicamento")]
        public string Name { get; set; }

        /// <summary>
        /// Laboratory Name
        /// </summary>
        [Display(Name = "Laboratório")]
        public string LaboratoryName { get; set; }
    }
}