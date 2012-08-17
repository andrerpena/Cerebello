using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Web.WebPages;
using CerebelloWebRole.Code.Extensions;

namespace CerebelloWebRole.Code.Controls
{
    public enum EditPanelFieldSize
    {
        Default = 0,
        Small,
        Large,
        XLarge
    }

    public class EditPanel<TModel>
    {
        private TModel Model { get; set; }

        public List<EditPanelFieldBase> Fields { get; set; }

        public String Title { get; set; }

        public HtmlHelper<TModel> HtmlHelper { get; set; }

        public int FieldsPerRow { get; set; }

        public bool IsChildPanel { get; set; }

        public EditPanel(HtmlHelper<TModel> htmlHelper, String title, int fieldsPerRow = 1, bool isChildPanel = false)
        {
            this.Fields = new List<EditPanelFieldBase>();
            this.HtmlHelper = htmlHelper;
            this.FieldsPerRow = fieldsPerRow;
            this.Model = htmlHelper.ViewData.Model;
            this.Title = title;
            this.IsChildPanel = isChildPanel;
        }

        public void Bind(TModel model)
        {
            this.Model = model;
        }

        public void AddField<TValue>(Expression<Func<TModel, TValue>> exp, Func<dynamic, object> format = null, Func<dynamic, object> formatDescription = null, string header = null, bool wholeRow = false)
        {
            this.Fields.Add(new EditPanelField<TModel, TValue>(exp, EditPanelFieldSize.Default, format, formatDescription, header, wholeRow));
        }

        public void AddField<TValue>(Expression<Func<TModel, TValue>> exp, EditPanelFieldSize size, Func<dynamic, object> format = null, Func<dynamic, object> formatDescription = null, string header = null, bool wholeRow = false)
        {
            this.Fields.Add(new EditPanelField<TModel, TValue>(exp, size, format, formatDescription, header, wholeRow));
        }

        public MvcHtmlString GetHtml(object htmlAttributes = null)
        {
            List<List<EditPanelFieldBase>> rows = new List<List<EditPanelFieldBase>>();
            // a primeira coisa a fazer é organizar os campos em rows
            {
                List<EditPanelFieldBase> currentRow = null;
                for (var i = 0; i < this.Fields.Count; i++)
                {
                    var field = this.Fields[i];

                    if (field.WholeRow)
                    {
                        // neste caso o campo ocupa a linha inteira e, se já existe um TR
                        // aberto, eu preciso fechar
                        currentRow = new List<EditPanelFieldBase>();
                        currentRow.Add(field);
                        rows.Add(currentRow);

                        // setando currentRow para null eu forço outro field
                        // a entrar em outra linha numa próxima interação
                        currentRow = null;
                    }

                    else if (currentRow == null || currentRow.Count == this.FieldsPerRow)
                    {
                        currentRow = new List<EditPanelFieldBase>();
                        currentRow.Add(field);
                        rows.Add(currentRow);
                    }
                    else
                        currentRow.Add(field);
                }
            }

            var wrapperDiv = new TagBuilder("div");
            wrapperDiv.AddCssClass("edit-panel-wrapper");
            if (this.IsChildPanel)
                wrapperDiv.AddCssClass("child-panel-wrapper");


            var table = new TagBuilder("table");
            var tableContentBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(this.Title))
            {
                var title = new TagBuilder("p");
                title.AddCssClass("title");
                title.SetInnerText(this.Title);
                tableContentBuilder.Append(title.ToString());
            }

            table.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            table.AddCssClass("edit-panel");
            if (this.IsChildPanel)
                table.AddCssClass("child-panel");

            foreach (var row in rows)
            {
                var tableTR = new TagBuilder("tr");
                var tableTRContentBuilder = new StringBuilder();

                for (var i = 0; i < row.Count; i++)
                {
                    var field = row[i];

                    // extrai as meta-informações sobre o modelo para tornar
                    // possível determinar se o usuário atual tem permissão para ver 
                    // este campo
                    var expressionPropertyValue = field.GetType().GetProperty("Expression").GetValue(field, null);
                    var funcType = expressionPropertyValue.GetType().GetGenericArguments()[0];
                    var valueType = funcType.GetGenericArguments()[1];
                    var propertyInfo = (PropertyInfo)((MemberExpression)expressionPropertyValue.GetType().GetProperty("Body").GetValue(expressionPropertyValue, null)).Member;

                    var tableHeaderTD = new TagBuilder("th");
                    tableHeaderTD.AddCssClass("header");
                    if (i == 0)
                        tableHeaderTD.AddCssClass("first");

                    var tableValueTD = new TagBuilder("td");
                    tableValueTD.AddCssClass("value");
                    if (i == 0)
                        tableValueTD.AddCssClass("first");

                    switch (field.Size)
                    {
                        case EditPanelFieldSize.Small:
                            tableValueTD.AddCssClass("small");
                            break;
                        case EditPanelFieldSize.Large:
                            tableValueTD.AddCssClass("large");
                            break;
                        case EditPanelFieldSize.XLarge:
                            tableValueTD.AddCssClass("x-large");
                            break;
                    }

                    var tableValueTDContent = new TagBuilder("div");
                    tableValueTDContent.AddCssClass("content");

                    var tableValueTDDescription = new TagBuilder("div");
                    tableValueTDDescription.AddCssClass("description");

                    if (i == row.Count - 1 && row.Count < this.FieldsPerRow)
                    {
                        tableValueTD.Attributes["colspan"] = (1 + (this.FieldsPerRow - row.Count) * 2).ToString();
                    }

                    // LabelFor
                    // public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression);
                    var labelForMethod =
                        (from m in
                             typeof(LabelExtensions).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                         where m.Name == "LabelFor"
                         select m).First();
                    var labelForMethodGeneric = labelForMethod.MakeGenericMethod(typeof(TModel), valueType);

                    MethodInfo editorForMethodGeneric = null;

                    // verifico as situações especiais
                    // 1) verifico se é um enum
                    if (propertyInfo.PropertyType.IsEnum || ((propertyInfo.PropertyType == typeof(int) ||
                        propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                        && propertyInfo.PropertyType.GetGenericArguments()[0] == typeof(int)) && propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), true).Length > 0))
                    {
                        var editorForMethod = (from m in typeof(HtmlExtensions).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                               where m.Name == "EnumDropdownListFor"
                                               select m).First();
                        editorForMethodGeneric = editorForMethod.MakeGenericMethod(typeof(TModel), valueType);
                    }
                    else
                    {
                        var editorForMethod = (from m in typeof(EditorExtensions).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                               where m.Name == "EditorFor"
                                               select m).First();
                        editorForMethodGeneric = editorForMethod.MakeGenericMethod(typeof(TModel), valueType);
                    }

                    var headerContent = field.Header != null ? new MvcHtmlString(field.Header).ToString() : labelForMethodGeneric.Invoke(null, new object[] { this.HtmlHelper, expressionPropertyValue }).ToString();
                    if (propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Length == 1 && !string.IsNullOrEmpty(headerContent))
                        headerContent += "<span class='required'>*</span>";

                    // table value td format
                    if (field.Format != null)
                        tableValueTDContent.InnerHtml = field.Format(this.Model).ToString();
                    else
                        tableValueTDContent.InnerHtml = editorForMethodGeneric.Invoke(null, new object[] { this.HtmlHelper, expressionPropertyValue }).ToString();

                    // table value td description format
                    if (field.FormatDescription != null)
                        tableValueTDDescription.InnerHtml = field.FormatDescription(this.Model).ToString().Trim();
                    else
                        tableValueTDDescription.InnerHtml = null;

                    tableHeaderTD.InnerHtml = headerContent;
                    tableValueTD.InnerHtml = tableValueTDContent.ToString() + (string.IsNullOrEmpty(tableValueTDDescription.InnerHtml) ? "" : tableValueTDDescription.ToString());

                    tableTRContentBuilder.Append((!string.IsNullOrEmpty(headerContent) ? tableHeaderTD.ToString() : "") + tableValueTD.ToString());
                }


                if (tableTRContentBuilder.Length > 0)
                {
                    tableTR.InnerHtml = tableTRContentBuilder.ToString();
                    tableContentBuilder.Append(tableTR.ToString());
                }
            }

            table.InnerHtml = tableContentBuilder.ToString();
            wrapperDiv.InnerHtml = table.ToString();

            return new MvcHtmlString(wrapperDiv.ToString());
        }
    }
}
