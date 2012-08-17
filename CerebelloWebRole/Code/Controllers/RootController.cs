using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Controllers
{
    public class RootController : Controller
    {
        public RootController()
        {
            this.UtcNowGetter = () => DateTime.UtcNow;
        }

        public Func<DateTime> UtcNowGetter { get; set; }

        /// <summary>
        /// Mockable version of the DateTime.UtcNow property.
        /// </summary>
        /// <returns></returns>
        public DateTime GetUtcNow()
        {
            return this.UtcNowGetter();
        }
    }
}