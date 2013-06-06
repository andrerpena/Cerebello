﻿using System;
using System.IO;
using System.Linq;

namespace CerebelloWebRole.Code.Services
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

        /// <summary>
        /// Moves a file from one place to another in the storage, if it exists.
        /// </summary>
        /// <param name="sourceFileLocation">Location of the file to move.</param>
        /// <param name="destinationFileLocation">Destination to move the file to.</param>
        /// <returns>True if the file was moved; otherwise false.</returns>
        public bool Move(string sourceFileLocation, string destinationFileLocation)
        {
            var destContainer = GetContainerName(destinationFileLocation);
            if (Directory.Exists(destContainer))
            {
                var destDir = Path.GetDirectoryName(destinationFileLocation);
                Directory.CreateDirectory(destDir);
            }

            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, sourceFileLocation);
            var destinationPath = Path.Combine(DebugConfig.LocalStoragePath, destinationFileLocation);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
            {
                fileInfo.MoveTo(destinationPath);
                return true;
            }

            return false;
        }

        public Stream OpenRead(string fileLocation)
        {
            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
                return fileInfo.OpenRead();

            return null;
        }

        /// <summary>
        /// Creates a container in the storage if possible, using the given location.
        /// </summary>
        /// <param name="location">A location that starts with a container name.</param>
        /// <returns>Returns true if directory was created; otherwise false.</returns>
        public bool CreateContainer(string location)
        {
            var path = Path.Combine(DebugConfig.LocalStoragePath, location);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes files in the storage.
        /// If it is a directory, then all files inside it will be deleted.
        /// Directories will never be left empty.
        /// </summary>
        /// <param name="location">The location of the files to delete.</param>
        public void DeleteFiles(string location)
        {
            var containerName = GetContainerName(location);
            var pathContainer = Path.Combine(DebugConfig.LocalStoragePath, containerName) + '\\';
            var path = Path.Combine(DebugConfig.LocalStoragePath, location);
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
        /// <param name="stream">The stream to save to the storage location.</param>
        /// <param name="fileLocation">Location in the storage to save the file to.</param>
        public void SaveFile(Stream stream, string fileLocation)
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

        public Stream OpenAppend(string fileLocation)
        {
            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Exists)
                return fileInfo.Open(FileMode.Append);

            return null;
        }

        public bool Exists(string fileLocation)
        {
            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            return File.Exists(sourcePath);
        }

        public Stream CreateOrOverwrite(string fileLocation)
        {
            // if container already exists, just create the directories inside
            var destContainer = GetContainerName(fileLocation);
            if (Directory.Exists(destContainer))
            {
                var destDir = Path.GetDirectoryName(fileLocation);
                Directory.CreateDirectory(destDir);
            }

            var sourcePath = Path.Combine(DebugConfig.LocalStoragePath, fileLocation);
            return File.Create(sourcePath);
        }
    }
}