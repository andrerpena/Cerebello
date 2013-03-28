using System.IO;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code.WindowsAzure
{
    public interface IWindowsAzureBlobStorageManager
    {
        /// <summary>
        /// Uploads a file to the Windows Azure Storage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        void UploadFileToStorage([NotNull] Stream stream, [NotNull] string containerName, [NotNull] string fileName);

        /// <summary>
        /// Downloads a file from the Windows Azure Storage
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        MemoryStream DownloadFileFromStorage([NotNull] string containerName, [NotNull] string fileName);

        /// <summary>
        /// Deletes a file from the Window Azure Storage
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        void DeleteFileFromStorage([NotNull] string containerName, [NotNull] string fileName);
    }
}