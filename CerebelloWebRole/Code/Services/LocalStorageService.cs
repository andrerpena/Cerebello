using System;
using System.IO;
using System.Linq;

namespace CerebelloWebRole.Code
{
    public class LocalStorageService : IStorageService
    {
        private static string GetContainerName(string location)
        {
            return location.Split(new[] { '\\' }, 2)[0];
        }

        /// <summary>
        /// Gets the length of a file in the storage, if it exists, otherwise null.
        /// </summary>
        /// <param name="fileLocation">Location of the file inside the storage.</param>
        /// <returns>The file length if it exists; otherwise null.</returns>
        public long? GetFileLength(string fileLocation)
        {
            var localPath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            var fileInfo = new FileInfo(localPath);
            if (fileInfo.Exists)
                return fileInfo.Length;

            return null;
        }

        public bool CopyFile(string fileLocation, string destinationFileLocation)
        {
            var destContainer = GetContainerName(destinationFileLocation);
            if (Directory.Exists(destContainer))
            {
                var destDir = Path.GetDirectoryName(destinationFileLocation);
                Directory.CreateDirectory(destDir);
            }

            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            var destinationPath = Path.Combine(DebugConfig.LocalStoragePath, destinationFileLocation);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
            {
                fileInfo.CopyTo(destinationPath);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves a file from one place to another in the storage, if it exists.
        /// </summary>
        /// <param name="fileLocation">Location of the file to move.</param>
        /// <param name="destinationFileLocation">Destination to move the file to.</param>
        /// <returns>True if the file was moved; otherwise false.</returns>
        public bool MoveFile(string fileLocation, string destinationFileLocation)
        {
            var destContainer = GetContainerName(destinationFileLocation);
            if (Directory.Exists(destContainer))
            {
                var destDir = Path.GetDirectoryName(destinationFileLocation);
                Directory.CreateDirectory(destDir);
            }

            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            var destinationPath = Path.Combine(DebugConfig.LocalStoragePath, destinationFileLocation);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
            {
                fileInfo.MoveTo(destinationPath);
                return true;
            }

            return false;
        }

        public Stream OpenFileForReading(string fileLocation)
        {
            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
                return fileInfo.OpenRead();

            return null;
        }

        /// <summary>
        /// Deletes a blob in the storage.
        /// Directories will not be left empty.
        /// </summary>
        /// <param name="fileLocation">The location of the blob to delete.</param>
        public void DeleteFileIfExists(string fileLocation)
        {
            var containerName = GetContainerName(fileLocation);
            var pathContainer = Path.Combine(DebugConfig.LocalStoragePath, containerName) + '\\';
            var path = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
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

        /// <summary>
        /// Saves the data in a stream in the storage, at the given location, 
        /// replacing any already existing file.
        /// </summary>
        /// <param name="fileLocation">Location in the storage to save the file to.</param>
        /// <param name="stream">The stream to save to the storage location.</param>
        public void CreateOrOverwriteFile(string fileLocation, Stream stream)
        {
            if (!fileLocation.Contains('\\'))
                throw new Exception("All files must be saved inside a container.");

            // if container already exists, just create the directories inside
            var destContainer = GetContainerName(fileLocation);
            var filePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            if (Directory.Exists(Path.Combine(DebugConfig.LocalStoragePath, destContainer)))
            {
                var destDir = Path.GetDirectoryName(filePath);
                if (destDir != null)
                    Directory.CreateDirectory(destDir);
            }

            using (var fs = File.Open(filePath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }

        public void CreateOrAppendToFile(string fileLocation, Stream stream)
        {
            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
            {
                using (var streamWrite = fileInfo.Open(FileMode.Append, FileAccess.Write))
                    stream.CopyTo(streamWrite);
            }
            else
            {
                using (var streamWrite = fileInfo.Open(FileMode.Create, FileAccess.Write))
                    stream.CopyTo(streamWrite);
            }
        }

        public bool FileExists(string fileLocation)
        {
            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            return File.Exists(sourcePath);
        }
    }
}
