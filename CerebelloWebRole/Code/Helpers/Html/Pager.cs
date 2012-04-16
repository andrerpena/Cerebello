using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CerebelloWebRole.Code
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString Pager(this HtmlHelper htmlHelper, int itemCount, int itemPerPage = 30, string pageParameterName = "page")
        {
            int currentPage;
            int pageCount = itemCount / itemPerPage;

            if (!int.TryParse(htmlHelper.ViewContext.HttpContext.Request[pageParameterName], out currentPage))
                currentPage = 1;

            if (currentPage < 1)
                currentPage = 1;
            else if (currentPage > pageCount)
                currentPage = pageCount;

            TagBuilder outerTag = new TagBuilder("div");
            StringBuilder outerTagContent = new StringBuilder();
            outerTag.AddCssClass("pager");

            if (currentPage != 1)
            {
                // neste caso existe o botão "anterior"
                TagBuilder previousButtonTag = new TagBuilder("a");
                previousButtonTag.AddCssClass("button-prev");
                previousButtonTag.SetInnerText("anterior");

                // seta a url
                UrlBuilder urlBuilder = new UrlBuilder(htmlHelper.ViewContext.HttpContext.Request);
                urlBuilder.SetParam(pageParameterName, (currentPage - 1).ToString());
                previousButtonTag.Attributes["href"] = urlBuilder.ToString();

                outerTagContent.Append(previousButtonTag.ToString());
            }

            TagBuilder currentPageTag = new TagBuilder("span");
            currentPageTag.AddCssClass("current-page");
            currentPageTag.SetInnerText(currentPage.ToString());
            outerTagContent.Append(currentPageTag.ToString());

            outerTagContent.Append("/");

            TagBuilder pageCountTag = new TagBuilder("span");
            pageCountTag.AddCssClass("page-count");
            pageCountTag.SetInnerText(pageCount.ToString());
            outerTagContent.Append(pageCountTag.ToString());

            if (currentPage != pageCount)
            {
                // neste caso existe o botão "anterior"
                TagBuilder nextButtonTag = new TagBuilder("a");
                nextButtonTag.AddCssClass("button-next");
                nextButtonTag.SetInnerText("próxima");

                // seta a url
                UrlBuilder urlBuilder = new UrlBuilder(htmlHelper.ViewContext.HttpContext.Request);
                urlBuilder.SetParam(pageParameterName, (currentPage + 1).ToString());
                nextButtonTag.Attributes["href"] = urlBuilder.ToString();

                outerTagContent.Append(nextButtonTag.ToString());
            }

            outerTag.InnerHtml = outerTagContent.ToString();
            return new MvcHtmlString(outerTag.ToString());
        }
    }
}
