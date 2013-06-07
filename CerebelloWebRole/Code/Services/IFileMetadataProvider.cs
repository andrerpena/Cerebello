using System;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public interface IFileMetadataProvider
    {
        FileMetadata[] GetByIds(int[] ids);
        FileMetadata GetById(int id);

        /// <summary>
        /// Gets all file-metadatas in the given container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>All file-metadatas in the given container.</returns>
        FileMetadata[] GetByContainerName(string containerName);

        /// <summary>
        /// Gets all files filtered by container name and by blob name.
        /// Blob name can be ended with '*' to indicate that it must start with the preceding string.
        /// </summary>
        /// <param name="containerName">The name of the container to get files from.</param>
        /// <param name="blobNameFilter">The name of the blob. A starts-with operation is supported by issuing a string ended with '*'.</param>
        /// <returns>Returns the file metadata entries of the filtered files.</returns>
        FileMetadata[] GetByContainerAndBlobName(string containerName, string blobNameFilter);

        FileMetadata Create(string containerName, string sourceFileName, string blobName, int? ownerUserId, string tag = null, bool formatWithId = false);
        FileMetadata CreateTemporary(string containerName, string sourceFileName, string blobName, DateTime expirationDate, int? ownerUserId, string tag = null, bool formatWithId = false);
        FileMetadata CreateRelated(int relatedMetadataId, string relationType, string containerName, string sourceFileName, string blobName, int? ownerUserId, string tag = null, bool formatWithId = false);

        void SaveChanges();
    }
}
