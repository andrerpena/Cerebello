using System;
using System.Web.Mvc;

namespace CerebelloWebRole.Code
{
    public abstract class GridFieldBase
    {
        public Func<dynamic, object> Format { get; set; }
        public String Header { get; set; }
        public bool CanSort { get; set; }
        public bool WordWrap { get; set; }
        public string CssClass { get; set; }

        public abstract string GetColumnHeader();
        public abstract Func<dynamic, object> GetDisplayTextFunction(HtmlHelper htmlHelper);
    }
}
