using System.IO;
using System.Net;

namespace CerebelloWebRole.Code
{
    public class HttpHelper {
        public static string MakeHttpGetRequest(string uri) {
            WebRequest request = WebRequest.Create(uri);
            string html;
            using (StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
                html = streamReader.ReadToEnd();

            return html;
        }

        public static void DownloadFile(string uri, string targetFilePath) {
            string targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            using(WebClient client = new WebClient()) {
                client.DownloadFile(uri, targetFilePath);
            }           
        }
    }
}
