using System.IO;

namespace CerebelloWebRole.Code
{
    public interface IStorageService
    {
        long? GetFileLength(string fileLocation);

        bool Move(string sourceFileLocation, string destinationFileLocation);

        void DeleteBlob(string location);

        bool Exists(string fileLocation);

        void SaveFile(Stream stream, string fileLocation);

        Stream OpenRead(string fileLocation);

        void AppendToFile(Stream stream, string fileLocation);
    }
}