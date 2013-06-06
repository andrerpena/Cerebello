using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;

namespace CerebelloWebRole.Code.Services
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

        /// <summary>
        /// Gets an existing container or creates a new one
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private static CloudBlobContainer GetOrCreateBlobContainer([NotNull] string containerName)
        {
            if (containerName == null) throw new ArgumentNullException("containerName");

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Gets a reference to a container.
            var container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            if (container.CreateIfNotExists())
            {
                // Setup the new container.
                container.SetPermissions(
                   new BlobContainerPermissions
                   {
                       PublicAccess = BlobContainerPublicAccessType.Off,
                   });
            }

            return container;
        }

        private CloudBlockBlob GetCloudBlockBlob(string fileLocation)
        {
            var containerName = GetContainerName(fileLocation);
            var containerRef = this.containersMap.GetOrAdd(containerName, GetOrCreateBlobContainer);
            var blobName = GetBlobName(fileLocation);
            var blobRef = containerRef.GetBlockBlobReference(blobName);
            return blobRef;
        }

        private readonly ConcurrentDictionary<string, CloudBlobContainer> containersMap
            = new ConcurrentDictionary<string, CloudBlobContainer>();

        private readonly ConcurrentDictionary<string, CloudBlockBlob> blobsMap
            = new ConcurrentDictionary<string, CloudBlockBlob>();

        private readonly ConcurrentDictionary<CloudBlockBlob, CloudBlockBlob> blobsFetchAttrMap
            = new ConcurrentDictionary<CloudBlockBlob, CloudBlockBlob>();

        private CloudBlockBlob GetCloudBlockBlob(string fileLocation, bool useCache, bool fetchAttributes = false)
        {
            var blobRef = useCache
                ? this.blobsMap.GetOrAdd(fileLocation, this.GetCloudBlockBlob)
                : this.GetCloudBlockBlob(fileLocation);

            if (fetchAttributes)
            {
                if (useCache)
                {
                    this.blobsFetchAttrMap.GetOrAdd(
                        blobRef,
                        blob =>
                        {
                            // requesting file properties
                            blob.FetchAttributes();
                            return blob;
                        });
                }
                else
                {
                    blobRef.FetchAttributes();
                }
            }

            return blobRef;
        }

        public long? GetFileLength(string fileLocation)
        {
            var blobRef = this.GetCloudBlockBlob(fileLocation, useCache: true, fetchAttributes: true);
            return blobRef.Properties.Length;
        }

        public bool Move(string sourceFileLocation, string destinationFileLocation)
        {
            var blobSrcRef = this.GetCloudBlockBlob(sourceFileLocation, useCache: true);
            var blobDstRef = this.GetCloudBlockBlob(destinationFileLocation, useCache: true);
            blobDstRef.StartCopyFromBlob(blobSrcRef);

            // requesting operation status
            // todo: measure copy speed, and fetch attributes with less frequency based on avg speed
            while (true)
            {
                blobDstRef.FetchAttributes();
                if (blobDstRef.CopyState.Status != CopyStatus.Pending)
                    break;

                // sleeping 1 second per remaining 50-megabytes (this is the rate of a commong hard drive)
                var sleep = (blobDstRef.CopyState.TotalBytes ?? 0 - blobDstRef.CopyState.BytesCopied ?? 0) / (50 * 1024000.0);
                if (sleep > 60)
                    Thread.Sleep(TimeSpan.FromSeconds(60));
                else if (sleep > 0.1)
                    Thread.Sleep(TimeSpan.FromSeconds(sleep));
            }

            return blobDstRef.CopyState.Status == CopyStatus.Success;
        }

        public Stream OpenRead(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public bool CreateContainer(string location)
        {
            var blobSrcRef = this.GetCloudBlockBlob(sourceFileLocation, useCache: true);
          
            var containerName = GetContainerName(location);
            var containerRef = this.containersMap.GetOrAdd(containerName, GetOrCreateBlobContainer);
            var blobName = GetBlobName(location);
            var blobRef = containerRef.GetBlockBlobReference(blobName);

            var dirName = Path.GetDirectoryName(blobName);

            var dirRef = containerRef.GetDirectoryReference(dirName);
            dirRef.ListBlobsSegmented()

            throw new NotImplementedException();
        }

        public void DeleteFiles(string location)
        {
            var blobSrcRef = this.GetCloudBlockBlob(sourceFileLocation, useCache:);

        }

        public void SaveFile(Stream stream, string fileLocation)
        {
            throw new NotImplementedException();
        }

        public Stream OpenAppend(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileLocation)
        {
            throw new NotImplementedException();
        }


        public Stream CreateOrOverwrite(string fileLocation)
        {
            throw new NotImplementedException();
        }
    }
}
