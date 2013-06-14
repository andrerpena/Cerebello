using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Web;
using Cerebello.Manager.SqlAzure.ImportExport;
using Cerebello.Manager;
using System.Runtime.Serialization;

namespace Cerebello.Manager
{
    class ImportExportHelper
    {
        public string EndPointUri { get; set; }
        public string StorageKey { get; set; }
        public string BlobUri { get; set; }
        public string DatabaseServerName { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseServerUserName { get; set; }
        public string DatabaseServerPassword { get; set; }

        public ImportExportHelper()
        {
            EndPointUri = "";
            this.DatabaseServerName = "";
            StorageKey = "";
            this.DatabaseName = "";
            this.DatabaseServerUserName = "";
            this.DatabaseServerPassword = "";
        }

        public string DoExport(string blobUri)
        {
            Console.Write(String.Format("Starting Export Operation - {0}\n\r", DateTime.Now));
            string requestGuid = null;
            bool exportComplete = false;
            string exportedBlobPath = null;

            //Setup Web Request for Export Operation
            WebRequest webRequest = WebRequest.Create(this.EndPointUri + @"/Export");
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.ContentType = @"application/xml";

            //Create Web Request Inputs - Blob Storage Credentials and Server Connection Info
            ExportInput exportInputs = new ExportInput
            {
                BlobCredentials = new BlobStorageAccessKeyCredentials
                {
                    StorageAccessKey = this.StorageKey,
                    Uri = String.Format(blobUri, this.DatabaseName, DateTime.UtcNow.Ticks.ToString())
                },
                ConnectionInfo = new ConnectionInfo
                {
                    ServerName = this.DatabaseServerName,
                    DatabaseName = this.DatabaseName,
                    UserName = this.DatabaseServerUserName,
                    Password = this.DatabaseServerPassword
                }
            };

            //Perform Web Request
            Console.WriteLine("Making Web Request For Export Operation...");
            Stream webRequestStream = webRequest.GetRequestStream();
            DataContractSerializer dataContractSerializer = new DataContractSerializer(exportInputs.GetType());
            dataContractSerializer.WriteObject(webRequestStream, exportInputs);
            webRequestStream.Close();

            //Get Response and Extract Request Identifier
            Console.WriteLine("Reading Response and extracting Export Request Identifier...");
            WebResponse webResponse = null;
            XmlReader xmlStreamReader = null;

            try
            {
                //Initialize the WebResponse to the response from the WebRequest
                webResponse = webRequest.GetResponse();

                xmlStreamReader = XmlReader.Create(webResponse.GetResponseStream());
                xmlStreamReader.ReadToFollowing("guid");
                requestGuid = xmlStreamReader.ReadElementContentAsString();
                Console.WriteLine(String.Format("Your Export Request Guid is: {0}", requestGuid));

                //Get Export Operation Status
                while (!exportComplete)
                {
                    Console.WriteLine("Checking export status...");
                    List<StatusInfo> statusInfoList = CheckRequestStatus(requestGuid);
                    Console.WriteLine(statusInfoList.FirstOrDefault().Status);

                    if (statusInfoList.FirstOrDefault().Status == "Failed")
                    {
                        Console.WriteLine(String.Format("Database export failed: {0}", statusInfoList.FirstOrDefault().ErrorMessage));
                        exportComplete = true;
                    }

                    if (statusInfoList.FirstOrDefault().Status == "Completed")
                    {
                        exportedBlobPath = statusInfoList.FirstOrDefault().BlobUri;
                        Console.WriteLine(String.Format("Export Complete - Database exported to: {0}\n\r", exportedBlobPath));
                        exportComplete = true;
                    }
                }
                return exportedBlobPath;
            }
            catch (WebException responseException)
            {
                Console.WriteLine("Request Falied:{0}", responseException.Message);
                if (responseException.Response != null)
                {
                    Console.WriteLine("Status Code: {0}", ((HttpWebResponse)responseException.Response).StatusCode);
                    Console.WriteLine("Status Description: {0}\n\r", ((HttpWebResponse)responseException.Response).StatusDescription);
                }
                return null;
            }
        }

        public bool DoImport(string blobUri)
        {
            Console.Write(String.Format("Starting Import Operation - {0}\n\r", DateTime.Now));
            string requestGuid = null;
            bool importComplete = false;

            //Setup Web Request for Import Operation
            WebRequest webRequest = WebRequest.Create(this.EndPointUri + @"/Import");
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.ContentType = @"application/xml";

            //Create Web Request Inputs - Database Size & Edition, Blob Store Credentials and Server Connection Info
            ImportInput importInputs = new ImportInput
            {
                AzureEdition = "Web",
                DatabaseSizeInGB = 1,
                BlobCredentials = new BlobStorageAccessKeyCredentials
                {
                    StorageAccessKey = this.StorageKey,
                    Uri = String.Format(blobUri, this.DatabaseName, DateTime.UtcNow.Ticks.ToString())
                },
                ConnectionInfo = new ConnectionInfo
                {
                    ServerName = this.DatabaseServerName,
                    DatabaseName = this.DatabaseName,
                    UserName = this.DatabaseServerUserName,
                    Password = this.DatabaseServerPassword
                }
            };

            //Perform Web Request
            Console.WriteLine("Making Web Request for Import Operation...");
            Stream webRequestStream = webRequest.GetRequestStream();
            DataContractSerializer dataContractSerializer = new DataContractSerializer(importInputs.GetType());
            dataContractSerializer.WriteObject(webRequestStream, importInputs);
            webRequestStream.Close();

            //Get Response and Extract Request Identifier
            Console.WriteLine("Serializing response and extracting guid...");
            WebResponse webResponse = null;
            XmlReader xmlStreamReader = null;

            try
            {
                //Initialize the WebResponse to the response from the WebRequest
                webResponse = webRequest.GetResponse();

                xmlStreamReader = XmlReader.Create(webResponse.GetResponseStream());
                xmlStreamReader.ReadToFollowing("guid");
                requestGuid = xmlStreamReader.ReadElementContentAsString();
                Console.WriteLine(String.Format("Request Guid: {0}", requestGuid));

                //Get Status of Import Operation
                while (!importComplete)
                {
                    Console.WriteLine("Checking status of Import...");
                    List<StatusInfo> statusInfoList = CheckRequestStatus(requestGuid);
                    Console.WriteLine(statusInfoList.FirstOrDefault().Status);

                    if (statusInfoList.FirstOrDefault().Status == "Failed")
                    {
                        Console.WriteLine(String.Format("Database import failed: {0}", statusInfoList.FirstOrDefault().ErrorMessage));
                        importComplete = true;
                    }

                    if (statusInfoList.FirstOrDefault().Status == "Completed")
                    {
                        Console.WriteLine(String.Format("Import Complete - Database imported to: {0}\n\r", statusInfoList.FirstOrDefault().DatabaseName));
                        importComplete = true;
                    }
                }
                return importComplete;
            }
            catch (WebException responseException)
            {
                Console.WriteLine("Request Falied: {0}", responseException.Message);
                {
                    Console.WriteLine("Status Code: {0}", ((HttpWebResponse)responseException.Response).StatusCode);
                    Console.WriteLine("Status Description: {0}\n\r", ((HttpWebResponse)responseException.Response).StatusDescription);
                }

                return importComplete;
            }
        }

        public List<StatusInfo> CheckRequestStatus(string requestGuid)
        {
            WebRequest webRequest = WebRequest.Create(this.EndPointUri + string.Format("/Status?servername={0}&username={1}&password={2}&reqId={3}",
                    HttpUtility.UrlEncode(this.DatabaseServerName),
                    HttpUtility.UrlEncode(this.DatabaseServerUserName),
                    HttpUtility.UrlEncode(this.DatabaseServerPassword),
                    HttpUtility.UrlEncode(requestGuid)));

            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.ContentType = @"application/xml";
            WebResponse webResponse = webRequest.GetResponse();
            XmlReader xmlStreamReader = XmlReader.Create(webResponse.GetResponseStream());
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(List<StatusInfo>));

            return (List<StatusInfo>)dataContractSerializer.ReadObject(xmlStreamReader, true);
        }

        /// <summary>
        /// Known DACWebService end points.
        /// </summary>
        public static class EndPoints
        {
            public static readonly string NorthCentralUS = "https://ch1prod-dacsvc.azure.com/DACWebService.svc";
            public static readonly string SouthCentralUS = "https://sn1prod-dacsvc.azure.com/DACWebService.svc";
            public static readonly string NorthEurope = "https://db3prod-dacsvc.azure.com/DACWebService.svc";
            public static readonly string WestEurope = "https://am1prod-dacsvc.azure.com/DACWebService.svc";
            public static readonly string EastAsia = "https://hkgprod-dacsvc.azure.com/DACWebService.svc";
            public static readonly string SoutheastAsia = "https://sg1prod-dacsvc.azure.com/DACWebService.svc";
            public static readonly string EastUS = "https://bl2prod-dacsvc.azure.com/DACWebService.svc";
            public static readonly string WestUS = "https://by1prod-dacsvc.azure.com/DACWebService.svc";
        }
    }
}
