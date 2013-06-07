using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class PersonFilesController : DoctorController
    {
        public ActionResult GetProfilePicture(int personId)
        {
            try
            {
                var person = this.db.Persons.First(p => p.Id == personId);

                if (person.PictureBlobName != null)
                {
                    var storageManager = new WindowsAzureBlobStorageManager();
                    var picture = storageManager.DownloadFileFromStorage(Constants.PERSON_PROFILE_PICTURE_CONTAINER_NAME, person.PictureBlobName);
                    return this.File(picture, "image/png", person.PictureBlobName);
                }

                // if the file does not exist, return a male or female image
                var pictureResource =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        person.Gender == (int)TypeGender.Male
                            ? "CerebelloWebRole.Resources.Male-Default-Profile.jpg"
                            : "CerebelloWebRole.Resources.Female-Default-Profile.jpg");

                return pictureResource != null ? this.File(pictureResource, "image/jpg") : null;
            }
            catch (Exception ex)
            {
                var pictureResource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CerebelloWebRole.Resources.Male-Default-Profile.jpg");
                return pictureResource != null ? this.File(pictureResource, "image/jpg") : null;
            }
        }

        public ActionResult DeleteProfilePicture(int personId)
        {
            var person = this.db.Persons.FirstOrDefault(p => p.Id == personId);

            try
            {
                if (person != null && person.PictureBlobName != null)
                {
                    var storageManager = new WindowsAzureBlobStorageManager();
                    storageManager.DeleteFileFromStorage(Constants.PERSON_PROFILE_PICTURE_CONTAINER_NAME, person.PictureBlobName);
                    person.PictureBlobName = null;
                    this.db.SaveChanges();
                    return this.Json(
                        new
                            {
                                success = true
                            },
                        JsonRequestBehavior.AllowGet);
                }
                return this.Json(
                        new
                        {
                            success = true
                        },
                        JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(
                    new
                    {
                        success = false,
                        message = ex.Message
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Sets a person picture as the specified tempFileName.
        /// The tempFileName will be COPIED from the "temp" Azure Storage container name
        /// tempFileName can be deleted afterwards
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="tempFileName"></param>
        /// <returns></returns>
        public JsonResult TransferPersonPictureFromTempContainer(int personId, string tempFileName)
        {
            try
            {
                var person = this.db.Persons.First(p => p.Id == personId);

                // download temp file
                var storageManager = new WindowsAzureBlobStorageManager();
                var tempFile = storageManager.DownloadFileFromStorage(Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME, tempFileName);

                // delete temp file
                storageManager.DeleteFileFromStorage(Constants.AZURE_STORAGE_TEMP_FILES_CONTAINER_NAME, tempFileName);

                // if person has a profile picture already, delete it
                if (person.PictureBlobName != null)
                    storageManager.DeleteFileFromStorage(Constants.PERSON_PROFILE_PICTURE_CONTAINER_NAME, person.PictureBlobName);

                // upload downloaded temp file to person profile
                var profilePictureBlobName = "profile_picture_" + personId;
                storageManager.UploadFileToStorage(tempFile, Constants.PERSON_PROFILE_PICTURE_CONTAINER_NAME, profilePictureBlobName);
                person.PictureBlobName = profilePictureBlobName;

                // this controller shouldn't know about patients but.. it's the easiest way to do this now
                if (person.Patients.Any())
                    person.Patients.First().IsBackedUp = false;

                this.db.SaveChanges();

                return this.Json(
                    new
                    {
                        success = true
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(
                    new
                    {
                        success = false,
                        message = ex.Message
                    }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}