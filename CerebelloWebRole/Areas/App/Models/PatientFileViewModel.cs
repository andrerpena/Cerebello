using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// The patient file view model.
    /// </summary>
    public class PatientFileViewModel : FileViewModel
    {
        /// <summary>
        /// Gets or sets the Id of the patient file.
        /// This is not the file metadata id.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the title given to the file by the user.
        /// </summary>
        [Display(Name = "File title")]
        public string FileTitle { get; set; }
    }
}