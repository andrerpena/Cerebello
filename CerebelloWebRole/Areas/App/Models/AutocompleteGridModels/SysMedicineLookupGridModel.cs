using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// GridModel for displaying the SYS_Medicine autocomplete
    /// </summary>
    public class SysMedicineLookupGridModel
    {
        /// <summary>
        /// SYS_Medicine Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Medicine name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Laboratory name
        /// </summary>
        public string LaboratoryName { get; set; }
    }
}
