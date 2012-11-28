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
    public class CardViewResponsive<TModel>
    {
        private TModel Model { get; set; }

        public List<CardViewFieldBase> Fields { get; set; }

        public HtmlHelper<TModel> HtmlHelper { get; set; }

        public int FieldsPerRow { get; set; }

        public CardViewResponsive(HtmlHelper<TModel> htmlHelper, int fieldsPerRow = 2)
        {
            this.Fields = new List<CardViewFieldBase>();
            this.HtmlHelper = htmlHelper;
            this.FieldsPerRow = fieldsPerRow;
            //this.Model = htmlHelper.ViewData.Model;
        }

        //public void Bind(TModel model)
        //{
        //    this.Model = model;
        //}

        /// <summary>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="exp"></param>
        /// <param name="format"></param>
        /// <param name="header"></param>
        /// <param name="wholeRow"></param>
        public void AddField<TValue>(Expression<Func<TModel, TValue>> exp, Func<TModel, object> format = null, string header = null, bool wholeRow = false)
        {
            this.Fields.Add(new CardViewField<TModel, TValue>(exp, format, header, wholeRow));
        }

        public void AddLinkField<TValue>(Expression<Func<TModel, TValue>> exp, [JetBrains.Annotations.AspMvcAction] string actionName, Func<TModel, object> routeValuesFunc, bool wholeRow = false)
        {
            this.AddLinkField(exp, actionName, null, routeValuesFunc);
        }

        public void AddLinkField<TValue>(Expression<Func<TModel, TValue>> exp, [JetBrains.Annotations.AspMvcAction] string actionName, [JetBrains.Annotations.AspMvcController] string controllerName, Func<TModel, object> routeValuesFunc, bool wholeRow = false)
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

                    return this.HtmlHelper.ActionLink(
                        string.Format("{0}", text), actionName, controllerName, routeValuesFunc(item), null);
                });
        }

        public MvcHtmlString GetHtml(object htmlAttributes = null)
        {
            List<List<CardViewFieldBase>> rows = new List<List<CardViewFieldBase>>();
            // a primeira coisa a fazer é organizar os campos em rows
            {
                List<CardViewFieldBase> currentRow = null;
                foreach (var field in this.Fields)
                {
                    if (field.WholeRow)
                    {
                        // neste caso o campo ocupa a linha inteira e, se já existe um TR
                        // aberto, eu preciso fechar
                        currentRow = new List<CardViewFieldBase> {field};
                        rows.Add(currentRow);

                        // setando currentRow para null eu forço outro field
                        // a entrar em outra linha numa próxima interação
                        currentRow = null;
                    }

                    else if (currentRow == null || currentRow.Count == this.FieldsPerRow)
                    {
                        currentRow = new List<CardViewFieldBase> {field};
                        rows.Add(currentRow);
                    }
                    else
                        currentRow.Add(field);
                }
            }

            var wrapperDiv = new TagBuilder("div");
            wrapperDiv.AddCssClass("cardview");
            var wrapperDivContentBuilder = new StringBuilder();

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                var divRow = new TagBuilder("div");
                divRow.AddCssClass("row");
                var divRowContentBuilder = new StringBuilder();

                foreach (var field in row)
                {
                    var divColumn = new TagBuilder("div");
                    divColumn.AddCssClass("span" + (field.WholeRow ? 1 : this.FieldsPerRow));

                    var tableHeaderTd = new TagBuilder("div");
                    tableHeaderTd.AddCssClass("header");
                    if (i == 0)
                        tableHeaderTd.AddCssClass("first");

                    var tableValueTd = new TagBuilder("div");
                    tableValueTd.AddCssClass("value");
                    if (i == 0)
                        tableValueTd.AddCssClass("first");

                    tableHeaderTd.InnerHtml = field.Label(this.HtmlHelper).ToString();
                    tableValueTd.InnerHtml = field.Display(this.HtmlHelper).ToString();

                    divColumn.InnerHtml = tableHeaderTd + tableValueTd.ToString();

                    divRowContentBuilder.Append(divColumn);
                }


                if (divRowContentBuilder.Length <= 0)
                    continue;

                divRow.InnerHtml = divRowContentBuilder.ToString();
                wrapperDivContentBuilder.Append(divRow);
            }

            wrapperDiv.InnerHtml = wrapperDivContentBuilder.ToString();
            return new MvcHtmlString(wrapperDiv.ToString());
        }
    }
}
