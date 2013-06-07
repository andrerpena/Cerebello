using System.IO;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    public interface IBlobStorageManager
    {
        /// <summary>
        /// Gets the length of a file if it exists.
        /// Null if the file does not exist.
        /// </summary>
        /// <param name="containerName">Name of the container storing the blob.</param>
        /// <param name="blobName">Name of the blob to get the length of.</param>
        /// <returns>Returns the length of the file if it exists, or null if it does not exist.</returns>
        long? GetFileLength([NotNull] string containerName, [NotNull] string blobName);

        /// <summary>
        /// Uploads a file to the Windows Azure Storage.
        /// </summary>
        /// <param name="stream">Stream containing the data to be saved into the file.</param>
        /// <param name="containerName">Name of the container to store the uploaded blob.</param>
        /// <param name="blobName">Name of the blob to save the contents to.</param>
        void UploadFileToStorage([NotNull] Stream stream, [NotNull] string containerName, [NotNull] string blobName);

        /// <summary>
        /// Downloads a file from the Windows Azure Storage.
        /// </summary>
        /// <param name="containerName">Name of the container where the blob resides.</param>
        /// <param name="blobName">Name of the blob to get the contents from.</param>
        /// <returns>Returns a valid stream that can be used to read file data, or null if the file does not exist.</returns>
        Stream DownloadFileFromStorage([NotNull] string containerName, [NotNull] string blobName);

        /// <summary>
        /// Deletes a file from the Window Azure Storage.
        /// </summary>
        /// <param name="containerName">Name of the container where the blob to delete is.</param>
        /// <param name="blobName">Name of the blob to delete.</param>
        void DeleteFileFromStorage([NotNull] string containerName, [NotNull] string blobName);
    }
}