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
        public string DropboxAuthorizationUrl { get; set; }

        /// <summary>
        /// If the dropbox is associated or not
        /// </summary>
        public bool DropboxAssociated { get; set; }

        /// <summary>
        /// Dropbox user e-mail
        /// </summary>
        public string DropboxEmail { get; set; }
    }
}