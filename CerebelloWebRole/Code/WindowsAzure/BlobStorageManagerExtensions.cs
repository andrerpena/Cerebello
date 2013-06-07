using System.IO;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    public static class BlobStorageManagerExtensions
    {
        /// <summary>
        /// Gets the length of a file if it exists.
        /// Null if the file does not exist.
        /// </summary>
        /// <param name="this">Blob storage manager to use.</param>
        /// <param name="location">Location of the blob to get the length of.</param>
        /// <returns>Returns the length of the file if it exists, or null if it does not exist.</returns>
        public static long? GetFileLength(this IBlobStorageManager @this, BlobLocation location)
        {
            return @this.GetFileLength(location.ContainerName, location.BlobName);
        }

        /// <summary>
        /// Deletes a file from the Window Azure Storage.
        /// </summary>
        /// <param name="this">Blob storage manager to use.</param>
        /// <param name="location">Location of the blob to delete.</param>
        public static void DeleteFileFromStorage(this IBlobStorageManager @this, BlobLocation location)
        {
            @this.DeleteFileFromStorage(location.ContainerName, location.BlobName);
        }

        /// <summary>
        /// Downloads a file from the Windows Azure Storage.
        /// </summary>
        /// <param name="this">Blob storage manager to use.</param>
        /// <param name="location">Location of the blob to download.</param>
        /// <returns>Returns a valid stream that can be used to read file data, or null if the file does not exist.</returns>
        public static Stream DownloadFileFromStorage(this IBlobStorageManager @this, BlobLocation location)
        {
            return @this.DownloadFileFromStorage(location.ContainerName, location.BlobName);
        }

        /// <summary>
        /// Uploads a file to the Windows Azure Storage.
        /// </summary>
        /// <param name="stream">Stream containing the data to be saved into the file.</param>
        /// <param name="this">Blob storage manager to use.</param>
        /// <param name="location">Location of the blob to upload to.</param>
        public static void UploadFileToStorage(this IBlobStorageManager @this, [NotNull] Stream stream, BlobLocation location)
        {
            @this.UploadFileToStorage(stream, location.ContainerName, location.BlobName);
        }
    }
}