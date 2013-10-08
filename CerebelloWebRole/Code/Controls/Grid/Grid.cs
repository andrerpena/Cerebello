using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace CerebelloWebRole.Code
{
    public class Grid<TModel>
    {
        private readonly HtmlHelper htmlHelper;
        private readonly IEnumerable<TModel> model;

        public List<GridFieldBase> Fields { get; set; }

        /// <summary>
        /// The number of rows
        /// </summary>
        public int? Count { get; set; }

        public int? RowsPerPage { get; set; }

        public Grid(HtmlHelper htmlHelper, IEnumerable<TModel> model, int? rowsPerPage = null, int? count = null)
        {
            if (rowsPerPage.HasValue != count.HasValue)
                throw new Exception("When rowsPerPage is set on a Grid, count must be set as well and vice-versa");

            this.htmlHelper = htmlHelper;
            this.model = model;
            this.Fields = new List<GridFieldBase>();
            this.RowsPerPage = rowsPerPage;
            this.Count = count;
        }

        public void AddField<TValue>(Expression<Func<TModel, TValue>> exp, Func<TModel, object> format = null, string header = null, bool canSort = false, bool wordWrap = false, string cssClass = null)
        {
            Func<dynamic, object> funcFormat = null;
            if (format != null)
                funcFormat = d => format((TModel)(((WebGridRow)d).Value));

            this.Fields.Add(
                new GridField<TModel, TValue>
                    {
                        Expression = exp,
                        Format = funcFormat,
                        Header = header,
                        CanSort = canSort,
                        WordWrap = wordWrap,
                        CssClass = cssClass,
                    });
        }

        public void AddLinkField<TValue>(Expression<Func<TModel, TValue>> exp, [JetBrains.Annotations.AspMvcAction] string actionName, Func<TModel, object> routeValuesFunc)
        {
            this.AddLinkField(exp, actionName, null, routeValuesFunc);
        }

        public void AddLinkField<TValue>(Expression<Func<TModel, TValue>> exp, [JetBrains.Annotations.AspMvcAction] string actionName, [JetBrains.Annotations.AspMvcController] string controllerName, Func<TModel, object> routeValuesFunc)
        {
            var textGetter = exp.Compile();

            this.AddField(
                exp,
                item =>
                {
                    var text = textGetter(item);
                    // if TEXT IS NULL, the ActionLink helper will trigger an exception, so, this code means to clarify the possible error
                    if (text == null)
                        throw new Exception(
                            "The expression used to return the link text returned a null value. If you are using, for instance, model => model.Name. Verify that model.Name does not return null");

                    return this.htmlHelper.ActionLink(
                        string.Format("{0}", text), actionName, controllerName, routeValuesFunc(item), null);
                });
        }

        public void AddEditField(
            [JetBrains.Annotations.AspMvcAction] string actionName,
            [JetBrains.Annotations.AspMvcController] string controllerName,
            Func<TModel, object> routeValuesFunc)
        {
            var urlHelper = new UrlHelper(this.htmlHelper.ViewContext.RequestContext);

            this.AddField<object>(
                null,
                item => new MvcHtmlString(string.Format(@"<a href=""{0}"" class=""grid-action-link icon-link-edit""></a>", urlHelper.Action(actionName, controllerName, routeValuesFunc(item)))),
                cssClass: "action-column");
        }


        public MvcHtmlString GetHtml(object htmlAttributes = null)
        {
            if(this.model == null)
                throw new Exception("Cannot render grid. Model is null");

            if (this.model.Any())
            {
                // the way the grid is bound depends on the "Count" property. If it has been set, then 
                // the grid will be virtually paged, otherwise it won't be paged at all

                WebGrid webGrid = null;

                if (this.Count.HasValue)
                {
                    Debug.Assert(this.RowsPerPage.HasValue);
                    webGrid = new WebGrid(canPage: true, sortFieldName: "SortBy", sortDirectionFieldName: "SortDirection", pageFieldName: "Page", rowsPerPage: this.RowsPerPage.Value);
                    webGrid.Bind((IEnumerable<dynamic>)this.model, null, false, this.Count.Value);
                }
                else
                {
                    webGrid = new WebGrid(canPage: false);
                    webGrid.Bind((IEnumerable<dynamic>)this.model);
                }

                var webGridColumns = new List<WebGridColumn>();

                foreach (var field in this.Fields)
                {
                    var columnHeader = field.GetColumnHeader();
                    var cellContentFunction = field.GetDisplayTextFunction(this.htmlHelper);

                    var cssClasses = new List<string>();
                    if (field.WordWrap)
                        cssClasses.Add("no-wrap-column");
                    if (field.CssClass != null)
                        cssClasses.Add(field.CssClass);

                    webGridColumns.Add(
                        webGrid.Column(
                            style: string.Join(" ", cssClasses),
                            header: columnHeader,
                            format: cellContentFunction,
                            canSort: field.CanSort));
                }

                return new MvcHtmlString(webGrid.GetHtml(
                        columns: webGridColumns,
                        tableStyle: "webgrid",
                        headerStyle: "webgrid-header",
                        footerStyle: "webgrid-footer",
                        alternatingRowStyle: "webgrid-alternating-row",
                        selectedRowStyle: "webgrid-selected-row",
                        rowStyle: "webgrid-row-style").ToString());
            }

            var noRecords = new TagBuilder("div");
            noRecords.AddCssClass("message-warning");
            noRecords.SetInnerText("No data");

            return new MvcHtmlString(noRecords.ToString());
        }
    }
}
