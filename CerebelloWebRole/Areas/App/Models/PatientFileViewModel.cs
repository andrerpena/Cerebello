using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CerebelloWebRole.Code.Model.Metadata;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// The patient file view model.
    /// </summary>
    public class PatientFileViewModel
    {
        /// <summary>
        /// Gets or sets the Id of the patient file.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the original file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the title given to the file by the user.
        /// </summary>
        [Display(Name = "Título do arquivo")]
        public string FileTitle { get; set; }

        /// <summary>
        /// Gets or sets the container used to store the file in the storage.
        /// </summary>
        public string FileContainer { get; set; }

        /// <summary>
        /// Gets or sets the length of the file.
        /// </summary>
        public long? FileLength { get; set; }

        public string FileLengthStr
        {
            get
            {
                if (this.FileLength == null)
                    return null;

                if (this.FileLength > 1000000000)
                    return (this.FileLength.Value / 1000000000.0).ToString("0.00 GB", CultureInfo.InvariantCulture);

                if (this.FileLength > 1000000)
                    return (this.FileLength.Value / 1000000.0).ToString("0.00 MB", CultureInfo.InvariantCulture);

                return (this.FileLength.Value / 1000.0).ToString("0.00 KB", CultureInfo.InvariantCulture);
            }
        }
    }
}