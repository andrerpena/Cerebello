using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
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

        /// <summary>
        /// Adds a field to the edit panel.
        /// </summary>
        /// <typeparam name="TValue">Type of value returned from the model property to add to the edit panel.</typeparam>
        /// <param name="expression">Expression that represents the property of the model to be added to the edit panel.</param>
        /// <param name="size"></param>
        /// <param name="editorFormat"></param>
        /// <param name="formatDescription"></param>
        /// <param name="header"></param>
        /// <param name="wholeRow"></param>
        public void AddField<TValue>(
            Expression<Func<TModel, TValue>> expression,
            EditPanelFieldSize size = EditPanelFieldSize.Default,
            Func<dynamic, object> editorFormat = null,
            Func<dynamic, object> formatDescription = null,
            string header = null,
            bool wholeRow = false)
        {
            this.Fields.Add(
                new EditPanelField<TModel, TValue>(expression, size, editorFormat, formatDescription, header, wholeRow));
        }

        /// <summary>
        /// Adds a field to the edit panel.
        /// </summary>
        /// <typeparam name="TValue">Type of value returned from the model property to add to the edit panel.</typeparam>
        /// <param name="expression">Expression that represents the property of the model to be added to the edit panel.</param>
        public void AddField<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            this.Fields.Add(new EditPanelField<TModel, TValue>(expression));
        }

        /// <summary>
        /// Adds a field to the edit panel, passing a method to render the editor.
        /// </summary>
        /// <typeparam name="TValue">Type of value returned from the model property to add to the edit panel.</typeparam>
        /// <param name="expression">Expression that represents the property of the model to be added to the edit panel.</param>
        /// <param name="expressionFormat">Delegate that renders the editor for the property represented by the expression.</param>
        /// <param name="size">[optional] Size of the editor.</param>
        public void AddField<TValue>(
            Expression<Func<TModel, TValue>> expression,
            Func<Expression<Func<TModel, TValue>>, object> expressionFormat,
            EditPanelFieldSize size = EditPanelFieldSize.Default)
        {
            if (expressionFormat == null)
                throw new ArgumentNullException("expressionFormat");

            var format = (Func<dynamic, object>)(d => expressionFormat(expression));
            this.Fields.Add(new EditPanelField<TModel, TValue>(expression, size, format));
        }

        public void AddTextField<TValue>(Expression<Func<TModel, TValue>> exp, Func<dynamic, object> format = null, Func<dynamic, object> formatDescription = null, string header = null, bool wholeRow = false)
        {
            this.Fields.Add(new EditPanelTextField<TModel, TValue>(exp, format, formatDescription, header, wholeRow));
        }

        public MvcHtmlString GetHtml(object htmlAttributes = null)
        {
            var rows = new List<List<EditPanelFieldBase>>();

            // a primeira coisa a fazer é organizar os campos em rows
            {
                List<EditPanelFieldBase> currentRow = null;
                foreach (var field in this.Fields)
                {
                    if (field.WholeRow)
                    {
                        // neste caso o campo ocupa a linha inteira e, se já existe um TR
                        // aberto, eu preciso fechar
                        currentRow = new List<EditPanelFieldBase> { field };
                        rows.Add(currentRow);

                        // setando currentRow para null eu forço outro field
                        // a entrar em outra linha numa próxima interação
                        currentRow = null;
                    }
                    else if (currentRow == null || currentRow.Count == this.FieldsPerRow)
                    {
                        currentRow = new List<EditPanelFieldBase> { field };
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
                var tableTr = new TagBuilder("tr");
                var tableTrContentBuilder = new StringBuilder();

                for (var i = 0; i < row.Count; i++)
                {
                    var field = row[i];

                    // extrai as meta-informações sobre o modelo para tornar
                    // possível determinar se o usuário atual tem permissão para ver 
                    // este campo
                    var expressionPropertyValue = field.GetType().GetProperty("Expression").GetValue(field, null);
                    var funcType = expressionPropertyValue.GetType().GetGenericArguments()[0];
                    var valueType = funcType.GetGenericArguments()[1];
                    var propertyInfo =
                        (PropertyInfo)
                        ((MemberExpression)
                         expressionPropertyValue.GetType().GetProperty("Body").GetValue(expressionPropertyValue, null))
                            .Member;

                    var tableHeaderTd = new TagBuilder("th");
                    tableHeaderTd.AddCssClass("header");
                    if (i == 0)
                        tableHeaderTd.AddCssClass("first");

                    var tableValueTd = new TagBuilder("td");
                    tableValueTd.AddCssClass("value");
                    if (i == 0)
                        tableValueTd.AddCssClass("first");

                    switch (field.Size)
                    {
                        case EditPanelFieldSize.Small:
                            tableValueTd.AddCssClass("small");
                            break;
                        case EditPanelFieldSize.Large:
                            tableValueTd.AddCssClass("large");
                            break;
                        case EditPanelFieldSize.XLarge:
                            tableValueTd.AddCssClass("x-large");
                            break;
                    }

                    var tableValueTdContent = new TagBuilder("div");
                    tableValueTdContent.AddCssClass("content");

                    var tableValueTdDescription = new TagBuilder("div");
                    tableValueTdDescription.AddCssClass("description");

                    if (i == row.Count - 1 && row.Count < this.FieldsPerRow)
                    {
                        tableValueTd.Attributes["colspan"] = (1 + (this.FieldsPerRow - row.Count) * 2).ToString();
                    }

                    // LabelFor
                    // public static MvcHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression);
                    var labelForMethod =
                        (from m in
                             typeof(LabelExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                         where m.Name == "LabelFor"
                         select m).First();
                    var labelForMethodGeneric = labelForMethod.MakeGenericMethod(typeof(TModel), valueType);

                    var headerContent = field.Header != null
                        ? new MvcHtmlString(field.Header).ToString()
                        : labelForMethodGeneric.Invoke(
                            null,
                            new object[]
                                {
                                    this.HtmlHelper,
                                    expressionPropertyValue
                                })
                            .ToString();

                    if (propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Length == 1 &&
                        !string.IsNullOrEmpty(headerContent))
                        headerContent += "<span class='required'>*</span>";

                    // in case it's a text field
                    if (field.GetType().GetGenericTypeDefinition() == typeof(EditPanelField<,>))
                    {
                        MethodInfo editorForMethodGeneric = null;

                        // verifico as situações especiais
                        // 1) verifico se é um enum
                        if (propertyInfo.PropertyType.IsEnum
                            || (
                                (propertyInfo.PropertyType == typeof(int)
                                || propertyInfo.PropertyType.IsGenericType
                                    && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                                    && propertyInfo.PropertyType.GetGenericArguments()[0] == typeof(int))
                                && propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), true).Length > 0))
                        {
                            var editorForMethod =
                                (from m in
                                     typeof(HtmlExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                 where m.Name == "EnumDropdownListFor"
                                 select m).First();
                            editorForMethodGeneric = editorForMethod.MakeGenericMethod(typeof(TModel), valueType);
                        }
                        else
                        {
                            var editorForMethod =
                                (from m in
                                     typeof(EditorExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                 where m.Name == "EditorFor"
                                 select m).First();
                            editorForMethodGeneric = editorForMethod.MakeGenericMethod(typeof(TModel), valueType);
                        }

                        // table value td editorFormat
                        tableValueTdContent.InnerHtml = field.Format != null
                            ? field.Format(this.Model).ToString()
                            : editorForMethodGeneric.Invoke(
                                null,
                                new object[]
                                    {
                                        this.HtmlHelper,
                                        expressionPropertyValue
                                    }).ToString();
                    }

                    // in case it's a text-field
                    if (field.GetType().GetGenericTypeDefinition() == typeof(EditPanelTextField<,>))
                    {

                        MethodInfo displayForMethodGeneric = null;

                        var displayForMethod =
                                (from m in
                                     typeof(DisplayExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                 where m.Name == "DisplayFor"
                                 select m).First();

                        displayForMethodGeneric = displayForMethod.MakeGenericMethod(typeof(TModel), valueType);

                        // table value td editorFormat
                        tableValueTdContent.AddCssClass("text-content");
                        tableValueTdContent.InnerHtml = field.Format != null
                                                            ? field.Format(this.Model).ToString()
                                                            : displayForMethodGeneric.Invoke(null,
                                                                                            new object[]
                                                                                                {
                                                                                                    this.HtmlHelper,
                                                                                                    expressionPropertyValue
                                                                                                }).ToString();
                    }

                    // table value td description format
                    tableValueTdDescription.InnerHtml = field.FormatDescription != null
                                                            ? field.FormatDescription(this.Model)
                                                                   .ToString()
                                                                   .Trim()
                                                            : null;

                    tableHeaderTd.InnerHtml = headerContent;
                    tableValueTd.InnerHtml = tableValueTdContent + (string.IsNullOrEmpty(tableValueTdDescription.InnerHtml) ? "" : tableValueTdDescription.ToString());

                    tableTrContentBuilder.Append((!string.IsNullOrEmpty(headerContent) ? tableHeaderTd.ToString() : "") + tableValueTd);
                }

                if (tableTrContentBuilder.Length > 0)
                {
                    tableTr.InnerHtml = tableTrContentBuilder.ToString();
                    tableContentBuilder.Append(tableTr);
                }
            }

            table.InnerHtml = tableContentBuilder.ToString();
            wrapperDiv.InnerHtml = table.ToString();

            return new MvcHtmlString(wrapperDiv.ToString());
        }
    }
}
