using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace CerebelloWebRole.Code.Controls
{
    public class CardViewField<TModel, TValue> : CardViewFieldBase
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }

        public CardViewField(Expression<Func<TModel, TValue>> exp, Func<dynamic, object> format = null, string header = null, bool foreverAlone = false)
        {
            this.Format = format;
            this.Expression = exp;
            this.Header = header;
            this.WholeRow = foreverAlone;
        }

        public override MvcHtmlString Label(HtmlHelper htmlHelper)
        {
            if (this.Header != null)
                return new MvcHtmlString(this.Header);

            return ((HtmlHelper<TModel>)htmlHelper).LabelFor(this.Expression);
        }

        public override MvcHtmlString Display(HtmlHelper htmlHelper)
        {
            if (this.Format != null)
                return new MvcHtmlString(this.Format(htmlHelper.ViewData.Model).ToString());

            return ((HtmlHelper<TModel>)htmlHelper).DisplayFor(this.Expression);
        }
    }
}
