using System;
using System.IO;
using JetBrains.Annotations;

namespace CerebelloWebRole.Code
{
    public struct BlobLocation
    {
        private readonly string containerName;
        private readonly string blobName;

        public BlobLocation([NotNull] string containerName, [NotNull] string blobName)
        {
            if (containerName == null) throw new ArgumentNullException("containerName");
            if (blobName == null) throw new ArgumentNullException("blobName");

            if (containerName.Contains("\\") || string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Invalid container name.", "containerName");

            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Invalid blob name.", "blobName");

            this.containerName = containerName;
            this.blobName = blobName;
        }

        public BlobLocation([NotNull] string fullBlobPath)
        {
            if (fullBlobPath == null) throw new ArgumentNullException("fullBlobPath");

            var split = fullBlobPath.Split(new[] { '\\' }, 2);

            if (split.Length < 2 || string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]))
                throw new ArgumentException("Invalid full blob path.", "fullBlobPath");

            this.containerName = split[0];
            this.blobName = split[1];
        }

        public string ContainerName
        {
            get { return this.containerName; }
        }

        public string BlobName
        {
            get { return this.blobName; }
        }

        public string FullName
        {
            get { return Path.Combine(this.containerName, this.blobName); }
        }

        public string FileName
        {
            get { return Path.GetFileName(this.blobName) ?? ""; }
        }

        public string DirectoryName
        {
            get { return Path.GetDirectoryName(this.blobName); }
        }

        public string DirectoryFullName
        {
            get { return Path.GetDirectoryName(Path.Combine(this.containerName, this.blobName)); }
        }

        public override string ToString()
        {
            return this.FullName;
        }
    }
}