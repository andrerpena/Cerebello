using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    public class LocalBlobStorageManager : IBlobStorageManager
    {
        /// <summary>
        /// Gets the length of a file if it exists.
        /// Null if the file does not exist.
        /// </summary>
        /// <param name="containerName">Name of the container storing the blob.</param>
        /// <param name="blobName">Name of the blob to get the length of.</param>
        /// <returns>Returns the length of the file if it exists, or null if it does not exist.</returns>
        public long? GetFileLength(string containerName, string blobName)
        {
            var localPath = Path.Combine(DebugConfig.LocalStoragePath, containerName, blobName);
            var fileInfo = new FileInfo(localPath);
            if (fileInfo.Exists)
                return fileInfo.Length;

            return null;
        }

        /// <summary>
        /// Uploads a file to the Windows Azure Storage.
        /// </summary>
        /// <param name="stream">Stream containing the data to be saved into the file.</param>
        /// <param name="containerName">Name of the container to store the uploaded blob.</param>
        /// <param name="blobName">Name of the blob to save the contents to.</param>
        public void UploadFileToStorage([NotNull] Stream stream, [NotNull] string containerName, [NotNull] string blobName)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (containerName == null) throw new ArgumentNullException("containerName");
            if (blobName == null) throw new ArgumentNullException("blobName");

            // if container already exists, just create the directories inside
            var filePath = Path.Combine(DebugConfig.LocalStoragePath, containerName, blobName);
            if (Directory.Exists(Path.Combine(DebugConfig.LocalStoragePath, filePath)))
            {
                var destDir = Path.GetDirectoryName(filePath);
                if (destDir != null)
                    Directory.CreateDirectory(destDir);
            }

            using (var fs = File.Open(filePath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }

        /// <summary>
        /// Downloads a file from the Windows Azure Storage.
        /// </summary>
        /// <param name="containerName">Name of the container where the blob resides.</param>
        /// <param name="blobName">Name of the blob to get the contents from.</param>
        /// <returns>Returns a valid stream that can be used to read file data, or null if the file does not exist.</returns>
        public Stream DownloadFileFromStorage(string containerName, string blobName)
        {
            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, containerName, blobName);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
                return fileInfo.OpenRead();

            return null;
        }

        /// <summary>
        /// Deletes a file from the Window Azure Storage.
        /// </summary>
        /// <param name="containerName">Name of the container where the blob to delete is.</param>
        /// <param name="blobName">Name of the blob to delete.</param>
        public void DeleteFileFromStorage(string containerName, string blobName)
        {
            var pathContainer = Path.Combine(DebugConfig.LocalStoragePath, containerName) + '\\';
            var path = Path.Combine(DebugConfig.LocalStoragePath, containerName, blobName);
            var dirInfo = new DirectoryInfo(path);
            var fileInfo = new FileInfo(path);

            while (true)
            {
                if (pathContainer.StartsWith(dirInfo.FullName))
                    break;

                if (fileInfo != null && fileInfo.Exists)
                {
                    fileInfo.Delete();
                    dirInfo = fileInfo.Directory;
                    fileInfo = null;
                }
                else if (dirInfo.Exists)
                {
                    dirInfo.Delete(true);
                    dirInfo = dirInfo.Parent;
                }
                else
                {
                    dirInfo = dirInfo.Parent;
                }

                if (dirInfo == null)
                    break;

                if (dirInfo.Exists && Directory.EnumerateFileSystemEntries(dirInfo.FullName).Any())
                    break;
            }
        }
    }
}
