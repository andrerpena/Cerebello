using System;
using System.IO;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class CameraWindowController : DoctorController
    {
        [HttpGet]
        public ActionResult CameraWindow()
        {
            return this.View();
        }

        [HttpPost]
        public JsonResult PostPicture(CameraWindowViewModel formModel)
        {
            try
            {
                var postedData = this.Request["image"];
                if (string.IsNullOrEmpty(postedData))
                    throw new Exception("Could not find the uploaded image");

                var data = postedData.Substring(22);
                var bytes = Convert.FromBase64String(data);
                var bytesStream = new MemoryStream(bytes);
                bytesStream.Seek(0, SeekOrigin.Begin);

                var tempFileName = Guid.NewGuid().ToString().ToLower();

                var storageManager = new WindowsAzureBlobStorageManager();
                storageManager.UploadFileToStorage(bytesStream, Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME, tempFileName);

                return this.Json(
                    new
                        {
                            success = true,
                            fileName = tempFileName,
                            containerName = Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME
                        });
            }
            catch (Exception ex)
            {
                return this.Json(
                    new
                        {
                            success = false,
                            message = ex.Message
                        });
            }
        }
    }
}