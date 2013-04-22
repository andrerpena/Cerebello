using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CerebelloWebRole.Code.WindowsAzure
{
    /// <summary>
    /// Helper class for working with the Windows Azure Storage.
    /// The usage of the class REQUIRES the existence of a storage connection string in
    /// the Windows Azure Cloud project called 'StorageConnectionString'
    /// </summary>
    public class WindowsAzureBlobStorageManager : IWindowsAzureBlobStorageManager
    {
        /// <summary>
        /// Gets an existing container or creates a new one
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private CloudBlobContainer GetOrCreateBlogContainer([NotNull] string containerName)
        {
            if (containerName == null) throw new ArgumentNullException("containerName");

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            var container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            container.SetPermissions(
                new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Blob
                    });

            return container;
        }

        /// <summary>
        /// Uploads a file to the Windows Azure Storage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        public void UploadFileToStorage([NotNull] Stream stream, [NotNull] string containerName, [NotNull] string fileName)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (containerName == null) throw new ArgumentNullException("containerName");
            if (fileName == null) throw new ArgumentNullException("fileName");

            if (RoleEnvironment.IsAvailable)
            {
                var container = this.GetOrCreateBlogContainer(containerName);

                // Retrieve reference to the blob
                var blockBlob = container.GetBlockBlobReference(fileName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                blockBlob.UploadFromStream(stream);
            }
            else
            {
                // todo: DEBUG: when not in Azure, save file locally
            }
        }

        /// <summary>
        /// Downloads a file from the Windows Azure Storage
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public MemoryStream DownloadFileFromStorage([NotNull] string containerName, [NotNull] string fileName)
        {
            if (containerName == null) throw new ArgumentNullException("containerName");
            if (fileName == null) throw new ArgumentNullException("fileName");

            var container = this.GetOrCreateBlogContainer(containerName);

            // Retrieve reference to the blob
            var blockBlob = container.GetBlockBlobReference(fileName);

            var result = new MemoryStream();

            blockBlob.DownloadToStream(result);

            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

        /// <summary>
        /// Deletes a file from the Window Azure Storage
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        public void DeleteFileFromStorage([NotNull] string containerName, [NotNull] string fileName)
        {
            if (containerName == null) throw new ArgumentNullException("containerName");
            if (fileName == null) throw new ArgumentNullException("fileName");

            var container = this.GetOrCreateBlogContainer(containerName);

            // Retrieve reference to a blob named "myblob.txt".
            var blockBlob = container.GetBlockBlobReference(fileName);

            // Delete the blob.
            blockBlob.Delete();
        }
    }
}