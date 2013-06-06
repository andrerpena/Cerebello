using System.IO;

namespace CerebelloWebRole.Code.Services
{
    public interface IStorageService
    {
        long? GetFileLength(string fileLocation);

        bool Move(string sourceFileLocation, string destinationFileLocation);

        bool CreateContainer(string location);

        void DeleteFiles(string location);

        bool Exists(string fileLocation);

        void SaveFile(Stream stream, string fileLocation);

        Stream CreateOrOverwrite(string fileLocation);

        Stream OpenRead(string fileLocation);

        Stream OpenAppend(string fileLocation);
    }
}