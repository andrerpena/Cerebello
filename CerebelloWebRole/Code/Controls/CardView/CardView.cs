using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
using System.Linq.Expressions;
using System.Reflection;

namespace CerebelloWebRole.Code.Controls
{
    public class CardView<TModel>
    {
        private TModel Model { get; set; }

        public List<CardViewFieldBase> Fields { get; set; }

        public HtmlHelper<TModel> HtmlHelper { get; set; }

        public int FieldsPerRow { get; set; }

        public CardView(HtmlHelper<TModel> htmlHelper, int fieldsPerRow = 2)
        {
            this.Fields = new List<CardViewFieldBase>();
            this.HtmlHelper = htmlHelper;
            this.FieldsPerRow = fieldsPerRow;
            this.Model = htmlHelper.ViewData.Model;
        }

        public void Bind(TModel model)
        {
            this.Model = model;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="exp"></param>
        /// <param name="format"></param>
        /// <param name="header"></param>
        /// <param name="wholeRow"></param>
        public void AddField<TValue>(Expression<Func<TModel, TValue>> exp, Func<dynamic, object> format = null, string header = null, bool wholeRow = false)
        {
            this.Fields.Add(new CardViewField<TModel, TValue>(exp, format, header, wholeRow));
        }

        public MvcHtmlString GetHtml(object htmlAttributes = null)
        {
            List<List<CardViewFieldBase>> rows = new List<List<CardViewFieldBase>>();
            // a primeira coisa a fazer é organizar os campos em rows
            {
                List<CardViewFieldBase> currentRow = null;
                for (var i = 0; i < this.Fields.Count; i++)
                {
                    var field = this.Fields[i];

                    if (field.ForeverAlone)
                    {
                        // neste caso o campo ocupa a linha inteira e, se já existe um TR
                        // aberto, eu preciso fechar
                        currentRow = new List<CardViewFieldBase>();
                        currentRow.Add(field);
                        rows.Add(currentRow);

                        // setando currentRow para null eu forço outro field
                        // a entrar em outra linha numa próxima interação
                        currentRow = null;
                    }

                    else if (currentRow == null || currentRow.Count == this.FieldsPerRow)
                    {
                        currentRow = new List<CardViewFieldBase>();
                        currentRow.Add(field);
                        rows.Add(currentRow);
                    }
                    else
                        currentRow.Add(field);
                }
            }

            var wrapperDiv = new TagBuilder("div");
            wrapperDiv.AddCssClass("cardview-wrapper");

            var table = new TagBuilder("table");
            var tableContentBuilder = new StringBuilder();

            table.AddCssClass("cardview");
            table.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                var tableTR = new TagBuilder("tr");
                var tableTRContentBuilder = new StringBuilder();

                for (var j = 0; j < row.Count; j++)
                {
                    var field = row[j];

                    var expressionPropertyValue = field.GetType().GetProperty("Expression").GetValue(field, null);
                    var funcType = expressionPropertyValue.GetType().GetGenericArguments()[0];
                    var valueType = funcType.GetGenericArguments()[1];
                    var propertyInfo = (PropertyInfo)((MemberExpression)expressionPropertyValue.GetType().GetProperty("Body").GetValue(expressionPropertyValue, null)).Member;

                    var tableHeaderTD = new TagBuilder("td");
                    tableHeaderTD.AddCssClass("header");
                    if (i == 0)
                        tableHeaderTD.AddCssClass("first");

                    var tableValueTD = new TagBuilder("td");
                    tableValueTD.AddCssClass("value");
                    if (i == 0)
                        tableValueTD.AddCssClass("first");

                    if (j == row.Count - 1 && row.Count < this.FieldsPerRow)
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
                    var labelForMethodGeneric = labelForMethod.MakeGenericMethod(this.Model.GetType(), valueType);

                    // DisplayFor
                    // public static MvcHtmlString DisplayFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression);
                    //DisplayExtensions.DisplayFor
                    var displayForMethod =
                        (from m in
                             typeof(DisplayExtensions).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                         where m.Name == "DisplayFor"
                         select m).First();
                    var displayForMethodGeneric = displayForMethod.MakeGenericMethod(this.Model.GetType(), valueType);

                    tableHeaderTD.InnerHtml = field.Header != null ? new MvcHtmlString(field.Header).ToString() : labelForMethodGeneric.Invoke(null, new object[] { this.HtmlHelper, expressionPropertyValue }).ToString();
                    tableValueTD.InnerHtml = field.Format != null ? new MvcHtmlString(field.Format(this.Model).ToString()).ToString() : displayForMethodGeneric.Invoke(null, new object[] { this.HtmlHelper, expressionPropertyValue }).ToString();

                    tableTRContentBuilder.Append(tableHeaderTD.ToString() + tableValueTD.ToString());
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
