using System;
using System.Collections.Generic;
using System.Globalization;
using Cerebello.Model;
using CerebelloWebRole.Code.Access;
using System.Linq;

namespace CerebelloWebRole.Code.Services
{
    public class DbFileMetadataProvider : IFileMetadataProvider
    {
        private readonly CerebelloEntitiesAccessFilterWrapper db;
        private readonly IDateTimeService dateTimeService;
        private readonly int practiceId;

        private readonly Queue<Action> actionsToSave = new Queue<Action>();

        public DbFileMetadataProvider(CerebelloEntitiesAccessFilterWrapper db, IDateTimeService dateTimeService, int practiceId)
        {
            this.db = db;
            this.dateTimeService = dateTimeService;
            this.practiceId = practiceId;
        }

        public FileMetadata Create(
            string containerName, string sourceFileName, string blobName, int? ownerUserId, string tag = null, bool formatWithId = false)
        {
            Creator creator = formatWithId ? (Creator)this.CreateFormat : (Creator)this.Create;
            return creator(containerName, sourceFileName, blobName, null, null, null, ownerUserId, tag);
        }

        public FileMetadata CreateTemporary(
            string containerName,
            string sourceFileName,
            string blobName,
            DateTime expirationDate,
            int? ownerUserId,
            string tag = null,
            bool formatWithId = false)
        {
            Creator creator = formatWithId ? (Creator)this.CreateFormat : (Creator)this.Create;
            return creator(containerName, sourceFileName, blobName, null, null, expirationDate, ownerUserId, tag);
        }

        public FileMetadata CreateRelated(
            int relatedMetadataId,
            string relationType,
            string containerName,
            string sourceFileName,
            string blobName,
            int? ownerUserId,
            string tag = null,
            bool formatWithId = false)
        {
            Creator creator = formatWithId ? (Creator)this.CreateFormat : (Creator)this.Create;
            return creator(containerName, sourceFileName, blobName, relatedMetadataId, relationType, null, ownerUserId, tag);
        }

        public void SaveChanges()
        {
            while (this.actionsToSave.Count > 0)
            {
                var action = this.actionsToSave.Dequeue();
                if (action != null)
                    action();
            }

            this.db.SaveChanges();
        }

        private FileMetadata CreateFormat(
            string containerName,
            string sourceFileName,
            string blobName,
            int? relatedMetadataId,
            string relationType,
            DateTime? expirationDate,
            int? ownerUserId,
            string tag = null)
        {
            const string idDependent = "{id}";

            var firstSaveContainerName = containerName.Contains("{id}");
            var firstSaveSourceFileName = sourceFileName.Contains("{id}");
            var firstSaveBlobName = blobName.Contains("{id}");

            var md = this.Create(
                firstSaveContainerName ? idDependent : containerName,
                firstSaveSourceFileName ? idDependent : sourceFileName,
                firstSaveBlobName ? idDependent : blobName,
                relatedMetadataId,
                relationType,
                expirationDate,
                ownerUserId,
                tag);

            this.actionsToSave.Enqueue(
                () =>
                {
                    if (firstSaveContainerName || firstSaveSourceFileName || firstSaveBlobName)
                    {
                        this.db.SaveChanges();

                        var idStr = md.Id.ToString(CultureInfo.InvariantCulture);

                        if (firstSaveContainerName)
                            md.ContainerName = containerName.Replace(idDependent, idStr);

                        if (firstSaveSourceFileName)
                            md.SourceFileName = sourceFileName.Replace(idDependent, idStr);

                        if (firstSaveBlobName)
                            md.BlobName = blobName.Replace(idDependent, idStr);
                    }
                });

            return md;
        }

        private FileMetadata Create(
            string containerName,
            string sourceFileName,
            string blobName,
            int? relatedMetadataId,
            string relationType,
            DateTime? expirationDate,
            int? ownerUserId,
            string tag = null)
        {
            var md = new FileMetadata
                {
                    CreatedOn = this.dateTimeService.UtcNow,
                    PracticeId = this.practiceId,
                    ContainerName = containerName,
                    SourceFileName = sourceFileName,
                    ExpirationDate = expirationDate,
                    BlobName = blobName,
                    RelatedFileMetadataId = relatedMetadataId,
                    RelationType = relationType,
                    OwnerUserId = ownerUserId,
                    Tag = tag,
                };

            this.actionsToSave.Enqueue(() => this.db.FileMetadatas.AddObject(md));

            return md;
        }

        private delegate FileMetadata Creator(
            string containerName,
            string sourceFileName,
            string blobName,
            int? relatedMetadataId,
            string relationType,
            DateTime? expirationDate,
            int? ownerUserId,
            string tag);

        public FileMetadata[] GetByIds(int[] ids)
        {
            var result = this.db.FileMetadatas
                .Where(f => ids.Contains(f.Id))
                .ToArray();

            return result;
        }

        public FileMetadata GetById(int id)
        {
            return this.db.FileMetadatas
                .SingleOrDefault(f => f.Id == id);
        }

        public FileMetadata[] GetByContainerName(string containerName)
        {
            var result = this.db.FileMetadatas
                .Where(f => f.ContainerName == containerName)
                .ToArray();

            return result;
        }

        public FileMetadata[] GetByContainerAndBlobName(string containerName, string blobNameFilter)
        {
            if (blobNameFilter.EndsWith("*"))
            {
                var startsWithFilter = blobNameFilter.Substring(0, blobNameFilter.Length - 1);

                var result = this.db.FileMetadatas
                    .Where(f => f.ContainerName == containerName)
                    .Where(f => f.BlobName.StartsWith(startsWithFilter))
                    .ToArray();

                return result;
            }
            else
            {
                var result = this.db.FileMetadatas
                    .Where(f => f.ContainerName == containerName)
                    .Where(f => f.BlobName == blobNameFilter)
                    .ToArray();

                return result;
            }
        }
    }
}
