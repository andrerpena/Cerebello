using System;
using System.Globalization;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// Base model for things that extends file behavior.
    /// </summary>
    public abstract class FileViewModel
    {
        /// <summary>
        /// Gets or sets the file metadata Id of the patient file.
        /// </summary>
        public int MetadataId { get; set; }

        /// <summary>
        /// Gets or sets the original file name.
        /// </summary>
        public string SourceFileName { get; set; }

        /// <summary>
        /// Gets or sets the container used to store the file in the storage.
        /// </summary>
        public string ContainerName { get; set; }

        public string BlobName { get; set; }

        /// <summary>
        /// Gets or sets the length of the file.
        /// </summary>
        public long? FileLength { get; set; }

        public DateTime? ExpirationDate { get; set; }

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