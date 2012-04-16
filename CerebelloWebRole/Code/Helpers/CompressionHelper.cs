using System.IO;
using System.IO.Compression;
using System.Text;

namespace CerebelloWebRole.Code
{
    public class CompressionHelper
    {
        public static string Inflate(string encoded)
        {
            return BytesToString(Inflate(StringToBytes(encoded)));
        }

        public static byte[] Inflate(byte[] encoded)
        {
            using (MemoryStream inputStream = new MemoryStream(encoded))
            {
                using (DeflateStream inflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    string inflatedString;
                    using (StreamReader sr = new StreamReader(inflateStream, Encoding.Default))
                    {
                        inflatedString = sr.ReadToEnd();
                    }
                    return StringToBytes(inflatedString);
                }
            }
        }

        public static string Deflate(string value)
        {
            return BytesToString(Deflate(StringToBytes(value)));
        }

        public static byte[] Deflate(byte[] bytes)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(outputStream, CompressionMode.Decompress))
                {
                    using (MemoryStream inputStream = new MemoryStream(bytes))
                    {
                        inputStream.WriteTo(deflateStream);
                    }

                    outputStream.Seek(0, SeekOrigin.Begin);

                    return outputStream.GetBuffer();
                }
            }
        }

        public static byte[] StringToBytes(string value)
        {
            return Encoding.Default.GetBytes(value);
        }

        public static string BytesToString(byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

    }
}
