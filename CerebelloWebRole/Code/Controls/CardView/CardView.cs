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

                    if (field.WholeRow)
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
            wrapperDiv.AddCssClass("cardview");
            var wrapperDivContentBuilder = new StringBuilder();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                var divRow = new TagBuilder("div");
                divRow.AddCssClass("row");
                var divRowContentBuilder = new StringBuilder();

                for (var j = 0; j < row.Count; j++)
                {
                    var field = row[j];

                    var divColumn = new TagBuilder("div");
                    divColumn.AddCssClass("span" + (field.WholeRow ? 1 : this.FieldsPerRow).ToString());

                    var tableHeaderTD = new TagBuilder("div");
                    tableHeaderTD.AddCssClass("header");
                    if (i == 0)
                        tableHeaderTD.AddCssClass("first");

                    var tableValueTD = new TagBuilder("div");
                    tableValueTD.AddCssClass("value");
                    if (i == 0)
                        tableValueTD.AddCssClass("first");

                    tableHeaderTD.InnerHtml = field.Label(this.HtmlHelper).ToString();
                    tableValueTD.InnerHtml = field.Display(this.HtmlHelper).ToString();

                    divColumn.InnerHtml = tableHeaderTD.ToString() + tableValueTD.ToString();

                    divRowContentBuilder.Append(divColumn.ToString());
                }


                if (divRowContentBuilder.Length > 0)
                {
                    divRow.InnerHtml = divRowContentBuilder.ToString();
                    wrapperDivContentBuilder.Append(divRow.ToString());
                }
            }

            wrapperDiv.InnerHtml = wrapperDivContentBuilder.ToString();
            return new MvcHtmlString(wrapperDiv.ToString());
        }
    }
}
