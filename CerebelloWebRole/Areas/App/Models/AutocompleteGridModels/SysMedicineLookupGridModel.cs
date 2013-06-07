using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// GridModel for displaying the SYS_Medicine autocomplete
    /// </summary>
    public class SysMedicineLookupGridModel
    {
        /// <summary>
        /// SYS_Medicine Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Medicine name
        /// </summary>
        [Display(Name = "Nome")]
        public string Name { get; set; }

        /// <summary>
        /// Laboratory name
        /// </summary>
        [Display(Name = "Laboratório")]
        public string LaboratoryName { get; set; }
    }
}
