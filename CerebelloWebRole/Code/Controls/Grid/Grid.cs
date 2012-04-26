using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Web.Helpers;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Code.Controls
{
    public class Grid<TModel>
    {
        private IEnumerable<TModel> Model { get; set; }

        public List<GridFieldBase> Fields { get; set; }

        public int Count { get; set; }

        public int RowsPerPage { get; set; }

        public Grid(IEnumerable<TModel> model, int count, int rowsPerPage = 30)
        {
            this.Model = model;
            this.Fields = new List<GridFieldBase>();
            this.RowsPerPage = rowsPerPage;
            this.Count = count;
        }

        public void AddField<TValue>(Expression<Func<TModel, TValue>> exp, Func<dynamic, object> format = null, string header = null, bool canSort = false, bool wordWrap = false, string cssClass = null)
        {
            this.Fields.Add(new GridField<TModel, TValue>(exp, format, header, canSort, wordWrap, cssClass));
        }

        public MvcHtmlString GetHtml(object htmlAttributes = null)
        {
            if (this.Model.Any())
            {
                WebGrid webGrid = new WebGrid(canPage: true, sortFieldName: "SortBy", sortDirectionFieldName: "SortDirection", pageFieldName: "Page", rowsPerPage: this.RowsPerPage);
                webGrid.Bind((IEnumerable<dynamic>)this.Model, null, false, this.Count);

                List<WebGridColumn> webGridColumns = new List<WebGridColumn>();

                foreach (var field in this.Fields)
                {
                    var expressionPropertyValue = field.GetType().GetProperty("Expression").GetValue(field, null);
                    var funcType = expressionPropertyValue.GetType().GetGenericArguments()[0];
                    var valueType = funcType.GetGenericArguments()[1];
                    var propertyInfo = (PropertyInfo)((MemberExpression)expressionPropertyValue.GetType().GetProperty("Body").GetValue(expressionPropertyValue, null)).Member;

                    string columnHeader;
                    if (field.Header != null)
                        columnHeader = field.Header;
                    else
                    {
                        var displayAttributes = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), true);
                        if (displayAttributes.Length > 0)
                        {
                            columnHeader = (displayAttributes[0] as DisplayAttribute).Name;
                            if (string.IsNullOrEmpty(columnHeader))
                                columnHeader = propertyInfo.Name;
                        }
                        else
                            columnHeader = propertyInfo.Name;
                    }

                    if (field.Format == null)
                        field.Format = x => propertyInfo.GetValue(x.Value, null);

                    List<string> cssClasses = new List<string>();
                    if (field.WordWrap)
                        cssClasses.Add("no-wrap-column");
                    if (field.CssClass != null)
                        cssClasses.Add(field.CssClass);

                    webGridColumns.Add(webGrid.Column(style: string.Join(" ", cssClasses), header: columnHeader, format: field.Format, canSort: field.CanSort));
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
            else
            {
                TagBuilder noRecords = new TagBuilder("div");
                noRecords.AddCssClass("empty-search-box");
                noRecords.SetInnerText("Não existem registros a serem exibidos");

                return new MvcHtmlString(noRecords.ToString());
            }
        }
    }
}
