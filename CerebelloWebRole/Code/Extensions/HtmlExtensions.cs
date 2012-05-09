using System.Text;
using System.Web.Mvc;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web.Mvc.Html;
using System.Web;
using System.Web.Script.Serialization;
using CerebelloWebRole.Code.Controls;
using CerebelloWebRole.Areas.App.Models;
using System.Collections;

namespace CerebelloWebRole.Code.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Ids não podem possuir caracteres "especiais". É preciso removê-los do "Name".
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static string Encode(string original)
        {
            return original.Replace('[', '_').Replace(']', '_').Replace('.', '_');
        }

        public static CardViewResponsive<TModel> CreateCardView<TModel>(this HtmlHelper<TModel> html)
        {
            return new CardViewResponsive<TModel>(html);
        }

        public static EditPanel<TModel> CreateEditPanel<TModel>(this HtmlHelper<TModel> html, object htmlAttributes = null, bool isChildPanel = false, int fieldsPerRow = 1)
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

            var propertyInfo = (PropertyInfo)((MemberExpression)expression.Body).Member;
            var enumType = EnumHelper.GetEnumDataTypeFromExpression(expression);

            var valueTypeDictionary = EnumHelper.GetValueDisplayDictionary(enumType);

            var model = html.ViewContext.ViewData.Model;

            var value = (int?)propertyInfo.GetValue(model, null);
            if (value.HasValue)
            {
                if (valueTypeDictionary.ContainsKey(value.Value))
                    return new MvcHtmlString(valueTypeDictionary[value.Value]);

                throw new Exception(String.Format("Integer value does not have a correspondent Enum value of the given enum type. Integer value: {0}. Enum type: {1}", value.Value, enumType.FullName));
            }

            return new MvcHtmlString(defaultLabel);
        }

        public static MvcHtmlString EnumDropdownFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var memberName = ((MemberExpression)expression.Body).Member.Name;
            var propertyInfo = typeof(TModel).GetProperty(memberName);

            Type enumType = null;

            if (propertyInfo.PropertyType.IsEnum)
                enumType = propertyInfo.PropertyType;
            else
            {
                var attributes = propertyInfo.GetCustomAttributes(typeof(EnumDataTypeAttribute), true);
                if (attributes.Length == 0)
                    throw new Exception("cannot resolve enum type");

                enumType = ((EnumDataTypeAttribute)attributes[0]).EnumType;
                if (enumType == null)
                    throw new Exception("cannot resolve enum type");
            }

            var enumValues = Enum.GetValues(enumType);
            var items = new List<SelectListItem>();

            foreach (var value in enumValues)
                items.Add(new SelectListItem() { Value = ((int)value).ToString(), Text = EnumHelper.GetText(value) });

            return html.DropDownList(memberName, items, "");
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
                if (attributes.Length == 0)
                    throw new Exception("cannot resolve enum type");

                enumType = ((EnumDataTypeAttribute)attributes[0]).EnumType;
                if (enumType == null)
                    throw new Exception("cannot resolve enum type");
            }

            var model = ((WebViewPage)html.ViewContext.View).Model;
            var modelValue = propertyInfo.GetValue(model, null);

            if (modelValue == null)
                return new MvcHtmlString("");

            return new MvcHtmlString(EnumHelper.GetText((int)modelValue, enumType));
        }

        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string optionLabel, object htmlAttributes)
        {
            var valueDisplayDictionary = EnumHelper.GetSelectListItems(EnumHelper.GetEnumDataTypeFromExpression(expression));
            return htmlHelper.DropDownListFor(expression, valueDisplayDictionary, optionLabel, htmlAttributes);
        }

        /// <summary>
        /// Lookup
        /// </summary>
        public static MvcHtmlString LookupFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, object>> expressionId, Expression<Func<TModel, string>> expressionText, string actionUrl)
        {
            return LookupGridFor<TModel, LookupRow>(htmlHelper, expressionId, expressionText, actionUrl, lr => lr.Id, lr => lr.Value);
        }

        /// <summary>
        /// Lookup grid
        /// </summary>
        public static MvcHtmlString LookupGridFor<TModel, TGridModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, object>> expressionId, Expression<Func<TModel, string>> expressionText, string actionUrl, Expression<Func<TGridModel, object>> expressionLookupId, Expression<Func<TGridModel, object>> expressionLookupText, params Expression<Func<TGridModel, object>>[] otherColumns)
        {
            if (((Object)expressionId) == null) throw new ArgumentNullException("expressionId");
            if (((Object)expressionText) == null) throw new ArgumentNullException("expressionText");
            if (String.IsNullOrEmpty(actionUrl)) throw new ArgumentException("The string cannot be null nor empty", "actionUrl");
            if (((Object)expressionLookupId) == null) throw new ArgumentNullException("expressionLookupId");
            if (((Object)expressionLookupText) == null) throw new ArgumentNullException("expressionLookupText");

            TagBuilder scriptTag = new TagBuilder("script");
            scriptTag.Attributes["type"] = "text/javascript";

            var idPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expressionId);
            var textPropertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expressionText);

            var htmlPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;

            var model = htmlHelper.ViewContext.ViewData.Model;
            var idPropertyValue = model != null ? idPropertyInfo.GetValue(model, null) : null;
            var textPropertyValue = model != null ? (string)textPropertyInfo.GetValue(model, null) : null;

            var inputHiddenId = string.IsNullOrEmpty(htmlPrefix) ? idPropertyInfo.Name : Encode(htmlPrefix + "." + idPropertyInfo.Name);
            var inputHiddenName = string.IsNullOrEmpty(htmlPrefix) ? idPropertyInfo.Name : (htmlPrefix + "." + idPropertyInfo.Name);
            var inputTextId = string.IsNullOrEmpty(htmlPrefix) ? textPropertyInfo.Name : Encode(htmlPrefix + "." + textPropertyInfo.Name);
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
                throw new Exception(string.Format("O lookup possui configurações inválidas. A coluna de texto não faz parte da lista de colunas. Lookup: '{0}'. Coluna de text: '{1}'", inputTextId, options.columnText));

            scriptTag.InnerHtml = string.Format("$(\"#{0}\").lookup({1});", inputTextId, new JavaScriptSerializer().Serialize(options));

            // renders

            var tagBuilder = new StringBuilder();

            var inputTextClasses = new List<string> { "lookup" };

            // determines if there's any validation issue
            if ((htmlHelper.ViewData.ModelState[inputTextName] != null && htmlHelper.ViewData.ModelState[inputTextName].Errors.Count > 0) ||
                (htmlHelper.ViewData.ModelState[inputHiddenName] != null && htmlHelper.ViewData.ModelState[inputHiddenName].Errors.Count > 0))
                inputTextClasses.Add("lookup-validation-error");

            tagBuilder.Append(htmlHelper.TextBoxFor(expressionText, new { @class = string.Join(" ", inputTextClasses.ToArray())}));
            tagBuilder.AppendLine();
            tagBuilder.Append(htmlHelper.HiddenFor(expressionId));
            tagBuilder.AppendLine();

            tagBuilder.Append(scriptTag.ToString());

            return new MvcHtmlString(tagBuilder.ToString());
        }


        /// <summary>
        /// Creates a collection editor for an N-Property
        /// </summary>
        public static MvcHtmlString CollectionEditorFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, ICollection>> expression, string collectionItemEditor)
        {
            var propertyInfo = PLKExpressionHelper.GetPropertyInfoFromMemberExpression(expression);
            var addAnotherLinkId = "add-another-to-" + propertyInfo.Name.ToLower();
            var listClass = propertyInfo.Name.ToLower() + "-list";

            var viewModel = new CollectionEditorViewModel()
            {
                ListParialViewName = collectionItemEditor,
                AddAnotherLinkId = addAnotherLinkId,
                ListClass = listClass,
                Items = html.ViewContext.ViewData.Model != null ? new ArrayList(expression.Compile()((TModel)html.ViewContext.ViewData.Model)) : new ArrayList()
            };

            return html.Partial("CollectionEditor", viewModel);
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

            itemIndex = html.ViewData.ContainsKey(JQueryTemplatingEnabledKey) ? "${index}" : GetCollectionItemIndex(collectionIndexFieldName);

            string collectionItemName = String.Format("{0}[{1}]", collectionName, itemIndex);

            var indexField = new TagBuilder("input");
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
            var viewData = new ViewDataDictionary<TCollectionItem>(modelDefaultValues) { { JQueryTemplatingEnabledKey, true } };
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
