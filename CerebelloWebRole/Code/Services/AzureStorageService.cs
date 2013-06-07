using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class AzureStorageService : IStorageService
    {
        private static string GetContainerName(string location)
        {
            return location.Split(new[] { '\\' }, 2)[0];
        }

        private static string GetBlobName(string location)
        {
            return location.Split(new[] { '\\' }, 2).Skip(1).FirstOrDefault() ?? "";
        }

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
        /// <param name="fileLocation">Location of the blob to get a blob object for.</param>
        /// <param name="fetchAttributes">Whether to fetch blob attributes or not.</param>
        /// <param name="createContainer">Whether to create the container or not.</param>
        /// <returns>Return the blob object for the given file location.</returns>
        private Blob GetCloudBlockBlob(string fileLocation, bool fetchAttributes = false, bool createContainer = false)
        {
            // getting the object representing the blob
            Container container = null;
            Blob blob;

            if (!this.blobsMap.TryGetValue(fileLocation, out blob))
            {
                // Gets the object representing the container.
                var containerName = GetContainerName(fileLocation);

                if (!this.containersMap.TryGetValue(containerName, out container))
                {
                    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    var containerRef = blobClient.GetContainerReference(containerName);
                    this.containersMap[containerName]
                        = container
                        = new Container { CloudBlobContainer = containerRef };
                }

                var blobName = GetBlobName(fileLocation);
                var blobRef = container.CloudBlobContainer.GetBlockBlobReference(blobName);
                this.blobsMap[fileLocation]
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

        public long? GetFileLength(string fileLocation)
        {
            var blob = this.GetCloudBlockBlob(fileLocation, fetchAttributes: true);
            if (blob.Exists == true)
                return blob.CloudBlockBlob.Properties.Length;
            return null;
        }

        private static bool InternalCopyBlob(Blob blobDst, Blob blobSrc)
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

        public bool CopyFile(string fileLocation, string destinationFileLocation)
        {
            var blobSrc = this.GetCloudBlockBlob(fileLocation);
            var blobDst = this.GetCloudBlockBlob(destinationFileLocation, createContainer: true);
            var isCopyOk = InternalCopyBlob(blobDst, blobSrc);
            return isCopyOk;
        }

        public bool MoveFile(string fileLocation, string destinationFileLocation)
        {
            var blobSrc = this.GetCloudBlockBlob(fileLocation);
            var blobDst = this.GetCloudBlockBlob(destinationFileLocation, createContainer: true);
            var isCopyOk = InternalCopyBlob(blobDst, blobSrc);

            // removing source blob if copy succeded
            if (isCopyOk)
            {
                blobSrc.CloudBlockBlob.DeleteIfExists();
                blobSrc.Exists = false;
                blobDst.Exists = true;
            }

            return isCopyOk;
        }

        public Stream OpenFileForReading(string fileLocation)
        {
            var blob = this.GetCloudBlockBlob(fileLocation);
            var stream = blob.CloudBlockBlob.OpenRead();
            blob.Exists = true;
            return stream;
        }

        public void DeleteFileIfExists(string fileLocation)
        {
            var blob = this.GetCloudBlockBlob(fileLocation);
            blob.CloudBlockBlob.DeleteIfExists();
            blob.Exists = false;
        }

        public void CreateOrOverwriteFile(string fileLocation, Stream stream)
        {
            var blob = this.GetCloudBlockBlob(fileLocation, createContainer: true);
            blob.CloudBlockBlob.UploadFromStream(stream);
            blob.Exists = true;
        }

        public void CreateOrAppendToFile(string fileLocation, Stream stream)
        {
            var blob = this.GetCloudBlockBlob(fileLocation, createContainer: true);
            var blobRef = blob.CloudBlockBlob;
            {
                var blockIds = new List<string>(blobRef.DownloadBlockList().Select(b => b.Name));
                var curId = blockIds.Count;

                // 4 MB buffer
                var buffer = new byte[4 * 1024000];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, 4 * 1024000);

                    if (bytesRead == -1)
                        break;

                    if (bytesRead > 0)
                    {
                        var memStream = new MemoryStream(buffer, 0, bytesRead);
                        var newId = Convert.ToBase64String(Encoding.Default.GetBytes(curId.ToString(CultureInfo.InvariantCulture)));
                        blobRef.PutBlock(newId, memStream, null);
                        blockIds.Add(newId);
                        curId++;
                    }
                }

                blobRef.PutBlockList(blockIds);

                blob.Exists = true;
            }
        }

        public bool FileExists(string fileLocation)
        {
            var blob = this.GetCloudBlockBlob(fileLocation);
            if (!blob.Exists.HasValue)
                blob.Exists = blob.CloudBlockBlob.Exists();
            return blob.Exists.Value;
        }
    }
}
