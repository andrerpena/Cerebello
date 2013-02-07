using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CerebelloWebRole.Code
{
    public class GravatarHelper
    {
        public const string Ampersand = "&";
        public const string BadgeSymbol = "&#9679;";

        public static String GetGravatarUrl(String gravatarEMailHash, Size size)
        {
            string sizeAsString;
            // this code CAN BE BETTER. I'm jot not feeling like fixing it right now
            switch (size)
            {
                case Size.s16:
                    sizeAsString = "16";
                    break;
                case Size.s24:
                    sizeAsString = "24";
                    break;
                case Size.s32:
                    sizeAsString = "32";
                    break;
                case Size.s64:
                    sizeAsString = "64";
                    break;
                case Size.s128:
                    sizeAsString = "128";
                    break;
                default:
                    throw new Exception("Size not supported");
            }

            if (Configuration.Instance.IsLocalPresentation && HttpContext.Current != null)
            {
                var path = "/Content/Local/GravatarImages/" + gravatarEMailHash + "_" + sizeAsString + ".jpeg";
                if (File.Exists(HttpContext.Current.Request.MapPath("~" + path)))
                    return path;

                path = "/Content/Local/GravatarImages/" + sizeAsString + ".png";
                if (File.Exists(HttpContext.Current.Request.MapPath("~" + path)))
                    return path;
            }

            return "http://www.gravatar.com/avatar/" + gravatarEMailHash + "?s=" + sizeAsString + GravatarHelper.Ampersand + "d=identicon" + GravatarHelper.Ampersand + "r=PG&d=mm";
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

        /// <summary>
        /// Gravatar image size
        /// </summary>
        public enum Size
        {
            s16,
            s24,
            s32,
            s64,
            s128
        }
    }

}
