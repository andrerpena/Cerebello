using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

namespace CerebelloWebRole.Code
{
    public class AjaxForm : IDisposable
    {
        private readonly FormContext originalFormContext;
        private readonly ViewContext viewContext;
        private readonly string placeHolderSelector;
        private readonly TextWriter writer;

        public AjaxForm(HtmlHelper htmlHelper, string actionName, string controllerName, string placeHolderSelector, FormMethod method, IDictionary<string, object> htmlAttributes)
        {
            var formUrl = UrlHelper.GenerateUrl(null, actionName, controllerName, new RouteValueDictionary(), htmlHelper.RouteCollection, htmlHelper.ViewContext.RequestContext, true);
            var tagBuilder = new TagBuilder("form");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("action", formUrl);
            tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(method), true);
            this.writer = htmlHelper.ViewContext.Writer;
            this.writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));
            this.viewContext = htmlHelper.ViewContext;
            this.placeHolderSelector = placeHolderSelector;
            this.originalFormContext = viewContext.FormContext;
        }

        public void Dispose()
        {
            this.writer.Write("</form>");

            var tagBuilder = new TagBuilder("script");
            tagBuilder.MergeAttribute("type", "text/javascript");
            tagBuilder.InnerHtml = string.Format(@"
                var $container = $(""{0}"");
                $('form', $container).ajaxForm({{
                    success: function (result) {{
                        $container.replaceWith(result);
                    }}
                }});", this.placeHolderSelector);
            this.writer.Write(tagBuilder.ToString());
            this.viewContext.FormContext = this.originalFormContext;
        }
    }
}