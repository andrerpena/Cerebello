using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CerebelloWebRole.Code.Access;

namespace CerebelloWebRole.Code.Helpers
{
    /// <summary>
    /// Helps working with files.
    /// </summary>
    public class FileHelper
    {
        const string DEFAULT_LOCAL_PATH = @"D:\Profile - MASB\Desktop\Cerebello.Debug\Storage\";

        /// <summary>
        /// Gets the length of a file in the storage, if it exists, otherwise null.
        /// </summary>
        /// <param name="fileLocation">Location of the file inside the storage.</param>
        /// <returns>The file length if it exists; otherwise null.</returns>
        public static long? GetFileLength(string fileLocation)
        {
            if (DebugConfig.IsDebug)
            {
                var localPath = Path.Combine(DEFAULT_LOCAL_PATH, fileLocation);
                var fileInfo = new FileInfo(localPath);
                if (fileInfo.Exists)
                    return fileInfo.Length;
            }
            else
            {
                throw new NotImplementedException();
            }

            return null;
        }

        /// <summary>
        /// Moves a file from one place to another in the storage, if it exists.
        /// </summary>
        /// <param name="sourceFileLocation">Location of the file to move.</param>
        /// <param name="destinationFileLocation">Destination to move the file to.</param>
        /// <returns>True if the file was moved; otherwise false.</returns>
        public static bool Move(string sourceFileLocation, string destinationFileLocation)
        {
            if (DebugConfig.IsDebug)
            {
                var sourcePath = Path.Combine(DEFAULT_LOCAL_PATH, sourceFileLocation);
                var destinationPath = Path.Combine(DEFAULT_LOCAL_PATH, destinationFileLocation);
                var fileInfo = new FileInfo(sourcePath);
                if (fileInfo.Exists)
                {
                    fileInfo.MoveTo(destinationPath);
                    return true;
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return false;
        }

        /// <summary>
        /// Deletes a file in the storage, if it exists.
        /// </summary>
        /// <param name="fileLocation">Location of the file to delete.</param>
        public static void Delete(string fileLocation)
        {
            if (DebugConfig.IsDebug)
            {
                var sourcePath = Path.Combine(DEFAULT_LOCAL_PATH, fileLocation);
                var fileInfo = new FileInfo(sourcePath);
                if (fileInfo.Exists)
                    fileInfo.Delete();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static Stream OpenRead(string fileLocation)
        {
            if (DebugConfig.IsDebug)
            {
                var sourcePath = Path.Combine(DEFAULT_LOCAL_PATH, fileLocation);
                var fileInfo = new FileInfo(sourcePath);
                if (fileInfo.Exists)
                    return fileInfo.OpenRead();
            }
            else
            {
                throw new NotImplementedException();
            }

            return null;
        }

        /// <summary>
        /// Creates a directory in the storage if possible.
        /// </summary>
        /// <param name="location"></param>
        public static void CreateDirectory(string location)
        {
            if (DebugConfig.IsDebug)
            {
                var path = Path.Combine(DEFAULT_LOCAL_PATH, location);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Deletes a directory and all of its contents if it exists.
        /// </summary>
        /// <param name="location">The location of the directory to delete.</param>
        public static void DeleteDirectory(string location)
        {
            if (DebugConfig.IsDebug)
            {
                var path = Path.Combine(DEFAULT_LOCAL_PATH, location);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Saves an HttpPostedFileBase in the storage, at the given location, replacing any already existing file.
        /// </summary>
        /// <param name="postedFile">The posted file to save to the storage location.</param>
        /// <param name="fileLocation">Location in the storage to save the file to.</param>
        public static void SavePostedFile(HttpPostedFileBase postedFile, string fileLocation)
        {
            if (DebugConfig.IsDebug)
            {
                if (!fileLocation.Contains('\\'))
                    throw new Exception("All files must be saved inside a container.");

                var filePath = Path.Combine(DEFAULT_LOCAL_PATH, fileLocation);
                postedFile.SaveAs(filePath);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static Stream OpenAppend(string fileLocation)
        {
            if (DebugConfig.IsDebug)
            {
                var sourcePath = Path.Combine(DEFAULT_LOCAL_PATH, fileLocation);
                var fileInfo = new FileInfo(sourcePath);
                if (fileInfo.Exists)
                    return fileInfo.Open(FileMode.Append);
            }
            else
            {
                throw new NotImplementedException();
            }

            return null;
        }
    }
}