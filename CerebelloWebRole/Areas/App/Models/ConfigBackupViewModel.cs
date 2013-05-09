using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ConfigBackupViewModel
    {
        /// <summary>
        /// Url for getting user authorization
        /// </summary>
        public string GoogleDriveAuthorizationUrl { get; set; }

        /// <summary>
        /// If the Google Drive is associated or not
        /// </summary>
        public bool GoogleDriveAssociated { get; set; }

        /// <summary>
        /// Google Drive user e-mail
        /// </summary>
        public string GoogleDriveEmail { get; set; }
    }
}