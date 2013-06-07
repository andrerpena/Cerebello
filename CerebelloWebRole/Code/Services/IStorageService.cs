using System.IO;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Represents the hability to store, retrieve and query information about files in a storage system.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets the length of a file if it exists.
        /// Null if the file does not exist.
        /// </summary>
        /// <param name="fileLocation">Location of the file to get the length of.</param>
        /// <returns>Returns the length of the file if it exists, or null if it does not exist.</returns>
        long? GetFileLength(string fileLocation);

        /// <summary>
        /// Moves a file to the destination location. Can be used to rename a file.
        /// </summary>
        /// <param name="fileLocation">The location of the file to be moved.</param>
        /// <param name="destinationFileLocation">The destination location of the file.</param>
        /// <returns>Returns true if the file was moved; otherwise false.</returns>
        bool MoveFile(string fileLocation, string destinationFileLocation);

        /// <summary>
        /// Copies a file to the destination location.
        /// </summary>
        /// <param name="fileLocation">The location of the file to be copied.</param>
        /// <param name="destinationFileLocation">The destination location of the file.</param>
        /// <returns>Returns true if the file was copied; otherwise false.</returns>
        bool CopyFile(string fileLocation, string destinationFileLocation);

        /// <summary>
        /// Deletes a file if it exists.
        /// </summary>
        /// <param name="fileLocation">Location of the file to delete.</param>
        void DeleteFileIfExists(string fileLocation);

        /// <summary>
        /// Determines whether a file exists or not.
        /// </summary>
        /// <param name="fileLocation">The file location to be tested.</param>
        /// <returns>Returns true if a file exists at the given location; otherwise false.</returns>
        bool FileExists(string fileLocation);

        /// <summary>
        /// Creates a file or overwrites it if it already exists.
        /// </summary>
        /// <param name="fileLocation">The location to create the file at.</param>
        /// <param name="stream">Stream containing the data to be saved into the file.</param>
        void CreateOrOverwriteFile(string fileLocation, Stream stream);

        /// <summary>
        /// Opens the file at the given location for reading.
        /// If it does not exists, null is returned.
        /// </summary>
        /// <param name="fileLocation">The location to read the file from.</param>
        /// <returns>Returns a valid stream that can be used to read file data, or null if the file does not exist.</returns>
        Stream OpenFileForReading(string fileLocation);

        /// <summary>
        /// Creates a file or appends to the file.
        /// </summary>
        /// <param name="fileLocation">The location of the file to append to.</param>
        /// <param name="stream">Stream containing the data to be appended to the file.</param>
        void CreateOrAppendToFile(string fileLocation, Stream stream);
    }
}