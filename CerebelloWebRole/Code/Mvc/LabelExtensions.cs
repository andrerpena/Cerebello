using System.Web.Mvc;
using System.Linq.Expressions;
using System;
using System.Web.Routing;
using System.Linq;

namespace CommonLib.Mvc
{

    public static class LabelExtensions
    {
        public static MvcHtmlString LabelFor<TModel, TValue>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            object htmlAttributes
        )
        {
            return LabelHelper(
                html,
                ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                ExpressionHelper.GetExpressionText(expression),
                htmlAttributes
            );
        }

        private static MvcHtmlString LabelHelper(
            HtmlHelper html,
            ModelMetadata metadata,
            string htmlFieldName,
            object htmlAttributes
        )
        {
            string resolvedLabelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (string.IsNullOrEmpty(resolvedLabelText))
            {
                return MvcHtmlString.Empty;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.SetInnerText(resolvedLabelText);
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }
    }

}