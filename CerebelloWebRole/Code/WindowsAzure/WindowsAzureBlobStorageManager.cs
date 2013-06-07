using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using BlobContainerPermissions = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPermissions;
using BlobContainerPublicAccessType = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType;
using CloudBlobContainer = Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer;
using CloudBlockBlob = Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob;
using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Helper class for working with the Windows Azure Storage.
    /// The usage of the class REQUIRES the existence of a storage connection string in
    /// the Windows Azure Cloud project called 'StorageConnectionString'
    /// </summary>
    public class WindowsAzureBlobStorageManager : IBlobStorageManager
    {
        class Container
        {
            public bool? Exists { get; set; }
            public CloudBlobContainer CloudBlobContainer { get; set; }
        }

        class Blob
        {
            public bool? Exists { get; set; }
            public bool AttributesFetched { get; set; }
            public Container Container { get; set; }
            public CloudBlockBlob CloudBlockBlob { get; set; }
        }

        private readonly Dictionary<string, Container> containersMap
            = new Dictionary<string, Container>();

        private readonly Dictionary<string, Blob> blobsMap
            = new Dictionary<string, Blob>();

        /// <summary>
        /// Gets a blob reference to work with.
        /// </summary>
        /// <param name="containerName">Name of the container storing the blob.</param>
        /// <param name="blobName">Name of the blob to look for.</param>
        /// <param name="fetchAttributes">Whether to fetch blob attributes or not.</param>
        /// <param name="createContainer">Whether to create the container or not.</param>
        /// <returns>Return the blob object for the given file location.</returns>
        private Blob GetCloudBlockBlob([NotNull] string containerName, [NotNull] string blobName, bool fetchAttributes = false, bool createContainer = false)
        {
            // getting the object representing the blob
            Container container = null;
            Blob blob;

            string fullPath = Path.Combine(containerName, blobName);

            if (!this.blobsMap.TryGetValue(fullPath, out blob))
            {
                // Gets the object representing the container.
                if (!this.containersMap.TryGetValue(containerName, out container))
                {
                    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var containerRef = blobClient.GetContainerReference(containerName);
                    this.containersMap[containerName]
                        = container
                        = new Container { CloudBlobContainer = containerRef };
                }

                var blobRef = container.CloudBlobContainer.GetBlockBlobReference(blobName);
                this.blobsMap[fullPath]
                    = blob
                    = new Blob { CloudBlockBlob = blobRef, Container = container, };
            }

            if (container == null)
                container = blob.Container;

            // creating container if needed
            if (createContainer && container.Exists != true)
            {
                if (container.CloudBlobContainer.CreateIfNotExists())
                {
                    container.CloudBlobContainer.SetPermissions(
                        new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off });
                    blob.Exists = false;
                }

                container.Exists = true;
            }

            // fetching blob attributes
            if (fetchAttributes && blob.Exists != false && blob.AttributesFetched != true)
            {
                bool ok = false;
                try
                {
                    blob.CloudBlockBlob.FetchAttributes();
                    ok = true;
                }
                catch (StorageClientException e)
                {
                    if (e.ErrorCode != StorageErrorCode.ResourceNotFound)
                        throw;
                }

                blob.Exists = ok;
                blob.AttributesFetched = ok;
            }

            return blob;
        }

        /// <summary>
        /// Gets the length of a file if it exists.
        /// Null if the file does not exist.
        /// </summary>
        /// <param name="containerName">Name of the container storing the blob.</param>
        /// <param name="blobName">Name of the blob to get the length of.</param>
        /// <returns>Returns the length of the file if it exists, or null if it does not exist.</returns>
        public long? GetFileLength([NotNull] string containerName, [NotNull] string blobName)
        {
            var blob = this.GetCloudBlockBlob(containerName, blobName, fetchAttributes: true);
            if (blob.Exists == true)
                return blob.CloudBlockBlob.Properties.Length;
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

            var blob = this.GetCloudBlockBlob(containerName, blobName, createContainer: true);

            // Create or overwrite the blob with contents from a stream.
            blob.CloudBlockBlob.UploadFromStream(stream);
            blob.Exists = true;
        }

        /// <summary>
        /// Downloads a file from the Windows Azure Storage.
        /// </summary>
        /// <param name="containerName">Name of the container where the blob resides.</param>
        /// <param name="blobName">Name of the blob to get the contents from.</param>
        /// <returns>Returns a valid stream that can be used to read file data, or null if the file does not exist.</returns>
        public Stream DownloadFileFromStorage([NotNull] string containerName, [NotNull] string blobName)
        {
            if (containerName == null) throw new ArgumentNullException("containerName");
            if (blobName == null) throw new ArgumentNullException("blobName");

            var blob = this.GetCloudBlockBlob(containerName, blobName);
            var stream = blob.CloudBlockBlob.OpenRead();
            blob.Exists = true;
            return stream;
        }

        /// <summary>
        /// Deletes a file from the Window Azure Storage.
        /// </summary>
        /// <param name="containerName">Name of the container where the blob to delete is.</param>
        /// <param name="blobName">Name of the blob to delete.</param>
        public void DeleteFileFromStorage([NotNull] string containerName, [NotNull] string blobName)
        {
            if (containerName == null) throw new ArgumentNullException("containerName");
            if (blobName == null) throw new ArgumentNullException("blobName");

            var blob = this.GetCloudBlockBlob(containerName, blobName);
            blob.CloudBlockBlob.DeleteIfExists();
            blob.Exists = false;
        }
    }
}