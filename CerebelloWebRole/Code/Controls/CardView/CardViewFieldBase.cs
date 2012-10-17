using System;
using System.Web.Mvc;

namespace CerebelloWebRole.Code.Controls
{
    public abstract class CardViewFieldBase
    {
        public Func<dynamic, object> Format { get; set; }
        public String Header { get; set; }

        /// <summary>
        /// ಠ.ಠ
        /// Determina se este campo aparece sozinho na linha
        /// </summary>
        public bool WholeRow { get; set; }

        public abstract MvcHtmlString Label(HtmlHelper htmlHelper);
        public abstract MvcHtmlString Display(HtmlHelper htmlHelper);
    }
}
