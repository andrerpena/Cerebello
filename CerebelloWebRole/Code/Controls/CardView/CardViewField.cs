using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace CerebelloWebRole.Code
{
    public class CardViewField<TModel, TValue> : CardViewFieldBase
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }

        public CardViewField(Expression<Func<TModel, TValue>> exp, Func<TModel, object> format = null, string header = null, bool foreverAlone = false)
        {
            Func<dynamic, object> dynFormat = null;
            if (format != null)
                dynFormat = d => format((TModel)d);

            this.Format = dynFormat;
            this.Expression = exp;
            this.Header = header;
            this.WholeRow = foreverAlone;
        }

        public override MvcHtmlString Label(HtmlHelper htmlHelper)
        {
            return this.Header != null
                ? new MvcHtmlString(this.Header)
                : ((HtmlHelper<TModel>)htmlHelper).LabelFor(this.Expression);
        }

        public override MvcHtmlString Display(HtmlHelper htmlHelper)
        {
            return this.Format != null
                ? new MvcHtmlString(this.Format(htmlHelper.ViewData.Model).ToString())
                : ((HtmlHelper<TModel>)htmlHelper).DisplayFor(this.Expression);
        }
    }
}
