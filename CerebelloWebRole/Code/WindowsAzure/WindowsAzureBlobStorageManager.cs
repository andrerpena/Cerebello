using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
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

            if (DebugConfig.IsDebug && !containerName.EndsWith("-debug"))
                containerName += "-debug";

            string fullPath = Path.Combine(containerName, blobName);

            if (!this.blobsMap.TryGetValue(fullPath, out blob))
            {
                // Gets the object representing the container.
                if (!this.containersMap.TryGetValue(containerName, out container))
                {
                    var storageAccountStr = StringHelper.FirstNonEmpty(
                        () => CloudConfigurationManager.GetSetting("StorageConnectionString"),
                        () => ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString,
                        () => ConfigurationManager.AppSettings["StorageConnectionString"],
                        () => { throw new Exception("No storage connection string found."); });

                    storageAccountStr = Regex.Replace(storageAccountStr, @"\s+", m => m.Value.Contains("\n") ? "" : m.Value);

                    var storageAccount = CloudStorageAccount.Parse(storageAccountStr);
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
                catch (StorageException e)
                {
                    if (e.ErrorCode != StorageErrorCode.ResourceNotFound)
                        throw;
                }
                catch (Microsoft.WindowsAzure.Storage.StorageException e)
                {
                    var webEx = e.InnerException as WebException;
                    if (webEx != null)
                    {
                        var httpWebResponse = webEx.Response as HttpWebResponse;
                        if (httpWebResponse != null && httpWebResponse.StatusCode != HttpStatusCode.NotFound)
                            throw;
                    }
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

            bool ok = blob.CloudBlockBlob.Exists();

            blob.Exists = ok;

            return ok ? stream : null;
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

        /// <summary>
        /// Copies a blob to another place.
        /// </summary>
        /// <param name="blobSrc">Source blob.</param>
        /// <param name="blobDst">Destination blob.</param>
        /// <returns>Returns true if the copy succeded; otherwise false.</returns>
        private static bool InternalCopyBlob(Blob blobSrc, Blob blobDst)
        {
            blobDst.CloudBlockBlob.StartCopyFromBlob(blobSrc.CloudBlockBlob);

            // requesting operation status
            // todo: measure copy speed, and fetch attributes with less frequency based on avg speed
            while (true)
            {
                blobDst.CloudBlockBlob.FetchAttributes();
                if (blobDst.CloudBlockBlob.CopyState.Status != CopyStatus.Pending)
                    break;

                // sleeping 1 second per remaining 50-megabytes (this is the rate of a commong hard drive)
                var sleep = (blobDst.CloudBlockBlob.CopyState.TotalBytes ?? 0 - blobDst.CloudBlockBlob.CopyState.BytesCopied ?? 0) /
                    (50 * 1024000.0);
                if (sleep > 60)
                    Thread.Sleep(TimeSpan.FromSeconds(60));
                else if (sleep > 0.1)
                    Thread.Sleep(TimeSpan.FromSeconds(sleep));
            }

            var isCopyOk = blobDst.CloudBlockBlob.CopyState.Status == CopyStatus.Success;

            return isCopyOk;
        }

        /// <summary>
        /// Copies a file stored in Windows Azure Storage to another blob.
        /// </summary>
        /// <param name="sourceContainerName">Name of the container where the source blob resides.</param>
        /// <param name="sourceBlobName">Name of the blob to copy data from.</param>
        /// <param name="destinationContainerName">Name of the container where the destination blob will be.</param>
        /// <param name="destinationBlobName">Name of the blob to copy data to.</param>
        public void CopyStoredFile(
            [NotNull] string sourceContainerName,
            [NotNull] string sourceBlobName,
            [NotNull] string destinationContainerName,
            [NotNull] string destinationBlobName)
        {
            if (sourceContainerName == null) throw new ArgumentNullException("sourceContainerName");
            if (sourceBlobName == null) throw new ArgumentNullException("sourceBlobName");
            if (destinationContainerName == null) throw new ArgumentNullException("destinationContainerName");
            if (destinationBlobName == null) throw new ArgumentNullException("destinationBlobName");

            var srcBlob = this.GetCloudBlockBlob(sourceContainerName, sourceBlobName);
            var dstBlob = this.GetCloudBlockBlob(destinationContainerName, destinationBlobName, createContainer: true);

            InternalCopyBlob(srcBlob, dstBlob);
        }
    }
}