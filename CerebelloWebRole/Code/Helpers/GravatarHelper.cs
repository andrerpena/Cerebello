using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace CerebelloWebRole.Code
{
    public class GravatarHelper
    {
        public const string Ampersand = "&";
        public const string BadgeSymbol = "&#9679;";

        public static String GetGravatarUrl(String gravatarEMailHash, Size size)
        {


            String sizeString = size == Size.s32 ? "32" : size == Size.s64 ? "64" : "128";
            return "http://www.gravatar.com/avatar/" + gravatarEMailHash + "?s=" + sizeString + GravatarHelper.Ampersand + "d=identicon" + GravatarHelper.Ampersand + "r=PG&d=mm";
        }

        // Create an md5 sum string of this string
        public static string GetGravatarHash(string email)
        {
            if (String.IsNullOrEmpty(email))
                email = "meu@email.com";

            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();  // Return the hexadecimal string.
        }

        delegate void DownloadGravatarDelegate(string gravatarID, int size, string targetFolderPath);
        public static void DownloadGravatar_Begin(string gravatarID, int size, string targetFolderPath)
        {
            AsyncHelper.FireAndForget(new DownloadGravatarDelegate(DownloadGravatar), gravatarID, size, targetFolderPath);
        }

        private static object _downloadLock = new object();

        public static void DownloadGravatar(string gravatarID, int size, string targetFilePath)
        {
            string gravatarPath = String.Format("http://www.gravatar.com/avatar.php?gravatar_id={0}" + GravatarHelper.Ampersand + "size={1}", gravatarID, size);

            if (!File.Exists(targetFilePath)) //TODO: GJ: there is a possible race condition here
                HttpHelper.DownloadFile(gravatarPath, targetFilePath);

            try
            { //Delete copy as we now have a fresh copy
                File.Delete(targetFilePath.Replace(".jpg", ".copy.jpg"));
            }
            catch (System.IO.IOException) { }

        }

        public enum Size
        {
            s32,
            s64,
            s128
        }
    }

}
