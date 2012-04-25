using System.Web.Mvc;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web.Mvc.Html;
using System.Web;
using System.Configuration;
using System.Linq;
using System.Web.Script.Serialization;
using System.Dynamic;
using CerebelloWebRole.Code.Controls;

namespace CerebelloWebRole.Code.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Ids não podem possuir caracteres "especiais". É preciso removê-los do "Name".
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static string ProcessHtmlId(string original)
        {
            return original.Replace('[', '_').Replace(']', '_').Replace('-', '_').Replace('.', '_');
        }

        public static CardViewResponsive<TModel> CreateCardView<TModel>(this HtmlHelper<TModel> html)
        {
            return new CardViewResponsive<TModel>(html);
        }

        public static EditPanel<TModel> CreateEditPanel<TModel>(this HtmlHelper<TModel> html, object htmlAttributes = null, bool isChildPanel = false, int fieldsPerRow = 2)
        {
            return new EditPanel<TModel>(html, null, isChildPanel: isChildPanel, fieldsPerRow: fieldsPerRow);
        }

        public static EditPanel<TModel> CreateEditPanel<TModel>(this HtmlHelper<TModel> html, string title)
        {
            return new EditPanel<TModel>(html, title);
        }

        public static Grid<TModel> CreateGrid<TModel, TViewModel>(this HtmlHelper<TViewModel> htmlHelper, IEnumerable<TModel> model, int count, int rowsPerPage)
        {
            return new Grid<TModel>(model, count, rowsPerPage);
        }

        public static MvcHtmlString ButtonLink(this HtmlHelper htmlHelper, string aText, string aUrl)
        {
            return new MvcHtmlString(String.Format("<input type=\"button\" class=\"button-link\" value=\"{0}\" href=\"{1}\" onclick=\"javascript:window.location.href=($(this).attr('href'))\" />", aText, aUrl));
        }

        public static MvcHtmlString DisplayEnumFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return DisplayEnumFor(html, expression, "");
        }

        public static MvcHtmlString DisplayEnumFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, String defaultLabel)
        {
            if (expression.Body.NodeType != ExpressionType.MemberAccess)
                throw new Exception("Expression must be an object member");

            var vPropertyInfo = (PropertyInfo)((MemberExpression)expression.Body).Member;
            var vEnumType = EnumHelper.GetEnumDataTypeFromExpression(expression);

            var vValueDisplayDictionary = EnumHelper.GetValueDisplayDictionary(vEnumType);

            var vModel = html.ViewContext.ViewData.Model;

            int? vValue = (int?)vPropertyInfo.GetValue(vModel, null);
            if (vValue.HasValue)
            {
                if (vValueDisplayDictionary.ContainsKey(vValue.Value))
                    return new MvcHtmlString(vValueDisplayDictionary[vValue.Value]);

                throw new Exception(String.Format("Integer value does not have a correspondent Enum value of the given enum type. Integer value: {0}. Enum type: {1}", vValue.Value, vEnumType.FullName));
            }

            return new MvcHtmlString(defaultLabel);
        }

        public static MvcHtmlString EnumDropdownFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var memberName = (expression.Body as MemberExpression).Member.Name;
            var propertyInfo = typeof(TModel).GetProperty(memberName);

            Type enumType = null;

            if (propertyInfo.PropertyType.IsEnum)
            {
                enumType = propertyInfo.PropertyType;
            }
            else
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), true);
                if (attributes == null || attributes.Length == 0)
                    throw new Exception("cannot resolve enum type");

                enumType = (attributes[0] as EnumDataTypeAttribute).EnumType;
                if (enumType == null)
                    throw new Exception("cannot resolve enum type");
            }

            var enumValues = Enum.GetValues(enumType);
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var value in enumValues)
                items.Add(new SelectListItem() { Value = ((int)value).ToString(), Text = EnumHelper.GetText(value) });

            return SelectExtensions.DropDownList(html, memberName, items, "");
        }

        public static MvcHtmlString EnumDisplayFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var propertyInfo = MemberExpressionHelper.GetPropertyInfo(expression);

            Type enumType = null;

            if (propertyInfo.PropertyType.IsEnum)
            {
                enumType = propertyInfo.PropertyType;
            }
            else
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), true);
                if (attributes == null || attributes.Length == 0)
                    throw new Exception("cannot resolve enum type");

                enumType = (attributes[0] as EnumDataTypeAttribute).EnumType;
                if (enumType == null)
                    throw new Exception("cannot resolve enum type");
            }

            var model = (html.ViewContext.View as WebViewPage).Model;
            var modelValue = propertyInfo.GetValue(model, null);

            if (modelValue == null)
                return new MvcHtmlString("");

            return new MvcHtmlString(EnumHelper.GetText((int)modelValue, enumType));
        }

        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string optionLabel, object htmlAttributes)
        {
            var valueDisplayDictionary = EnumHelper.GetSelectListItems(EnumHelper.GetEnumDataTypeFromExpression(expression));
            return SelectExtensions.DropDownListFor(htmlHelper, expression, valueDisplayDictionary, optionLabel, htmlAttributes);
        }

        /// <summary>
        /// Lookup
        /// </summary>
        public static MvcHtmlString LookupFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, int?>> expressionId, Expression<Func<TModel, string>> expressionText, string lookupId, string actionUrl)
        {
            return LookupGridFor<TModel, LookupRow>(htmlHelper, expressionId, expressionText, lookupId, actionUrl, lr => lr.Id, lr => lr.Value);
        }

        /// <summary>
        /// Lookup grid
        /// </summary>
        public static MvcHtmlString LookupGridFor<TModel, TGridModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, int?>> expressionId, Expression<Func<TModel, string>> expressionText, string lookupId, string actionUrl, Expression<Func<TGridModel, object>> expressionLookupId, Expression<Func<TGridModel, object>> expressionLookupText, params Expression<Func<TGridModel, object>>[] otherColumns)
        {
            if (((Object)expressionId) == null) throw new ArgumentNullException("expressionId");
            if (((Object)expressionText) == null) throw new ArgumentNullException("expressionText");
            if (((Object)lookupId) == null) throw new ArgumentNullException("lookupId");
            if (String.IsNullOrEmpty(actionUrl)) throw new ArgumentException("The string cannot be null nor empty", "actionUrl");
            if (((Object)expressionLookupId) == null) throw new ArgumentNullException("expressionLookupId");
            if (((Object)expressionLookupText) == null) throw new ArgumentNullException("expressionLookupText");

            var htmlPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;

            TagBuilder tagBuilder = new TagBuilder("div");
            var lookupIdFixed = string.IsNullOrEmpty(htmlPrefix) ? ProcessHtmlId(lookupId) : ProcessHtmlId(htmlPrefix + "." + lookupId);
            var lookupName = string.IsNullOrEmpty(htmlPrefix) ? lookupId : htmlPrefix + "." + lookupId;

            tagBuilder.Attributes["id"] = lookupIdFixed;
            tagBuilder.Attributes["data-val-name"] = lookupName;

            TagBuilder scriptTag = new TagBuilder("script");
            scriptTag.Attributes["type"] = "text/javascript";

            var idPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expressionId);
            var textPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expressionText);

            var model = htmlHelper.ViewContext.ViewData.Model;
            var idPropertyValue = model != null ? (int?)idPropertyInfo.GetValue(model, null) : null;
            var textPropertyValue = model != null ? (string)textPropertyInfo.GetValue(model, null) : null;

            var inputHiddenId = string.IsNullOrEmpty(htmlPrefix) ? idPropertyInfo.Name : ProcessHtmlId(htmlPrefix + "." + idPropertyInfo.Name);
            var inputHiddenName = string.IsNullOrEmpty(htmlPrefix) ? idPropertyInfo.Name : (htmlPrefix + "." + idPropertyInfo.Name);
            var inputTextId = string.IsNullOrEmpty(htmlPrefix) ? textPropertyInfo.Name : ProcessHtmlId(htmlPrefix + "." + textPropertyInfo.Name);
            var inputTextName = string.IsNullOrEmpty(htmlPrefix) ? textPropertyInfo.Name : (htmlPrefix + "." + textPropertyInfo.Name);

            var options = new LookupOptions()
            {
                contentUrl = actionUrl,
                inputHiddenId = inputHiddenId,
                inputHiddenName = inputHiddenName,
                inputHiddenValue = idPropertyValue,
                inputTextId = inputTextId,
                inputTextName = inputTextName,
                inputTextValue = textPropertyValue
            };

            var columnIdPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expressionLookupId);
            options.columnId = columnIdPropertyInfo.Name;

            // coluna de texto
            var columnTextPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expressionLookupText);
            options.columnText = columnTextPropertyInfo.Name;
            options.columns.Add(options.columnText);
            options.columnHeaders.Add(PLKExpressionHelper.GetDisplayName(expressionLookupText));

            // adiciona os nomes das colunas
            foreach (var columnExpression in otherColumns)
            {
                var otherColumnPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(columnExpression);
                options.columns.Add(otherColumnPropertyInfo.Name);
                options.columnHeaders.Add(PLKExpressionHelper.GetDisplayName(columnExpression));
            }

            // validação das colunas
            if (!options.columns.Contains(options.columnText))
                throw new Exception(string.Format("O lookup possui configurações inválidas. A coluna de texto não faz parte da lista de colunas. Lookup: '{0}'. Coluna de text: '{1}'", lookupIdFixed, options.columnText));

            scriptTag.InnerHtml = string.Format("$(\"#{0}\").lookup({1});", lookupIdFixed, new JavaScriptSerializer().Serialize(options));

            // validation
            if ((htmlHelper.ViewData.ModelState[inputTextName] != null && htmlHelper.ViewData.ModelState[inputTextName].Errors.Count > 0) ||
                (htmlHelper.ViewData.ModelState[inputHiddenName] != null && htmlHelper.ViewData.ModelState[inputHiddenName].Errors.Count > 0))
                tagBuilder.Attributes["class"] = "lookup-validation-error";

            return new MvcHtmlString(tagBuilder.ToString() + "\n" + scriptTag.ToString());
        }

        /// <summary>
        /// Begins a collection item by inserting either a previously used .Index hidden field value for it or a new one.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="html"></param>
        /// <param name="collectionName">The name of the collection property from the Model that owns this item.</param>
        /// <returns></returns>
        public static IDisposable BeginCollectionItem<TModel>(this HtmlHelper<TModel> html, string collectionName)
        {
            if (String.IsNullOrEmpty(collectionName))
                throw new ArgumentException("collectionName is null or empty.", "collectionName");

            string collectionIndexFieldName = String.Format("{0}.Index", collectionName);

            string itemIndex = null;
            if (html.ViewData.ContainsKey(JQueryTemplatingEnabledKey))
            {
                itemIndex = "${index}";
            }
            else
            {
                itemIndex = GetCollectionItemIndex(collectionIndexFieldName);
            }

            string collectionItemName = String.Format("{0}[{1}]", collectionName, itemIndex);

            TagBuilder indexField = new TagBuilder("input");
            indexField.MergeAttributes(new Dictionary<string, string>() {
                { "name", collectionIndexFieldName },
                { "value", itemIndex },
                { "type", "hidden" },
                { "autocomplete", "off" }
            });

            html.ViewData.Add(new KeyValuePair<string, object>("collectionIndex", itemIndex));
            html.ViewContext.Writer.WriteLine(indexField.ToString(TagRenderMode.SelfClosing));
            return new CollectionItemNamePrefixScope(html.ViewData.TemplateInfo, collectionItemName);
        }

        private const string JQueryTemplatingEnabledKey = "__BeginCollectionItem_jQuery";

        public static MvcHtmlString CollectionItemJQueryTemplate<TModel, TCollectionItem>(this HtmlHelper<TModel> html,
                                                                                            string partialViewName,
                                                                                            TCollectionItem modelDefaultValues)
        {
            ViewDataDictionary<TCollectionItem> viewData = new ViewDataDictionary<TCollectionItem>(modelDefaultValues);
            viewData.Add(JQueryTemplatingEnabledKey, true);
            return html.Partial(partialViewName, modelDefaultValues, viewData);
        }

        /// <summary>
        /// Tries to reuse old .Index values from the HttpRequest in order to keep the ModelState consistent
        /// across requests. If none are left returns a new one.
        /// </summary>
        /// <param name="collectionIndexFieldName"></param>
        /// <returns>a GUID string</returns>
        private static string GetCollectionItemIndex(string collectionIndexFieldName)
        {
            Queue<string> previousIndices = (Queue<string>)HttpContext.Current.Items[collectionIndexFieldName];
            if (previousIndices == null)
            {
                HttpContext.Current.Items[collectionIndexFieldName] = previousIndices = new Queue<string>();

                string previousIndicesValues = HttpContext.Current.Request[collectionIndexFieldName];
                if (!String.IsNullOrWhiteSpace(previousIndicesValues))
                {
                    foreach (string index in previousIndicesValues.Split(','))
                        previousIndices.Enqueue(index);
                }
            }

            return previousIndices.Count > 0 ? previousIndices.Dequeue() : Guid.NewGuid().ToString();
        }

        private class CollectionItemNamePrefixScope : IDisposable
        {
            private readonly TemplateInfo _templateInfo;
            private readonly string _previousPrefix;

            public CollectionItemNamePrefixScope(TemplateInfo templateInfo, string collectionItemName)
            {
                this._templateInfo = templateInfo;

                _previousPrefix = templateInfo.HtmlFieldPrefix;
                templateInfo.HtmlFieldPrefix = collectionItemName;
            }

            public void Dispose()
            {
                _templateInfo.HtmlFieldPrefix = _previousPrefix;
            }
        }
    }
}
