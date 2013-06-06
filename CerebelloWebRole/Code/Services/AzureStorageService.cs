using System;
using System.IO;

namespace CerebelloWebRole.Code.Services
{
    public class AzureStorageService : IStorageService
    {
        public long? GetFileLength(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public bool Move(string sourceFileLocation, string destinationFileLocation)
        {
            throw new NotImplementedException();
        }

        public void Delete(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public bool CreateContainer(string location)
        {
            throw new NotImplementedException();
        }

        public void DeleteFiles(string location)
        {
            throw new NotImplementedException();
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
