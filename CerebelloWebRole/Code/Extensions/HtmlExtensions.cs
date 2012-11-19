using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Script.Serialization;
using CerebelloWebRole.Areas.App.Models;
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

        /// <summary>
        /// Creates a grid considering the current view Model as a type identifier
        /// </summary>
        public static Grid<TModel> CreateGrid<TModel, TViewModel>(this HtmlHelper<TViewModel> htmlHelper, IEnumerable<TModel> model, int rowsPerPage, int? count = null)
        {
            return new Grid<TModel>(htmlHelper, model, rowsPerPage, count);
        }

        /// <summary>
        /// Displays an inline message-box, containing arbitrary text.
        /// The text will be html encoded.
        /// </summary>
        public static MvcHtmlString Message(this HtmlHelper htmlHelper, string text)
        {
            var encodedText = HttpUtility.HtmlEncode(text);
            return new MvcHtmlString(String.Format(@"<div class=""message-warning"">{0}</div>", encodedText));
        }

        /// <summary>
        /// Displays an inline message-box for help.
        /// This should be called in the side-boxes
        /// </summary>
        public static MvcHtmlString MessageHelp(this HtmlHelper htmlHelper, string text)
        {
            var encodedText = HttpUtility.HtmlEncode(text);
            return new MvcHtmlString(String.Format(@"<div class=""message-help"">{0}</div>", encodedText));
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

        public static MvcHtmlString EnumDisplayFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
            where TModel : class
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

            var model = html.ViewData.Model;

            if (model == null)
                return new MvcHtmlString("");

            var modelValue = propertyInfo.GetValue(model, null);

            if (modelValue == null)
                return new MvcHtmlString("");

            return new MvcHtmlString(EnumHelper.GetText((int)modelValue, enumType));
        }

        /// <summary>
        /// Shows a DropdownListFor automatically filled with enums values from the given expression
        /// </summary>
        /// <remarks>
        /// DO NOT remove this method and merge it with the OTHER OVERLOAD, using optional parameters.
        /// This method is required WITH THIS EXACT SIGNATURE in order to be called by the EditPanel
        /// </remarks>
        public static MvcHtmlString EnumDropdownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var valueDisplayDictionary = EnumHelper.GetSelectList(EnumHelper.GetEnumDataTypeFromExpression(expression), null);
            return htmlHelper.DropDownListFor(expression, valueDisplayDictionary, "");
        }

        /// <summary>
        /// Shows a DropdownListFor automatically filled with enums values from the given expression
        /// </summary>
        /// <remarks>
        /// DO NOT remove this method and merge it with the OTHER OVERLOAD, using optional parameters.
        /// The other method is required WITH THAT EXACT SIGNATURE in order to be called by the EditPanel
        /// </remarks>
        public static MvcHtmlString EnumDropdownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object selection, string optionLabel = "")
        {
            var valueDisplayDictionary = EnumHelper.GetSelectList(EnumHelper.GetEnumDataTypeFromExpression(expression), selection);
            return htmlHelper.DropDownListFor(expression, valueDisplayDictionary, optionLabel);
        }

        /// <summary>
        /// Lookup
        /// </summary>
        public static MvcHtmlString AutocompleteFor<TModel, TId>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TId>> expressionId, Expression<Func<TModel, string>> expressionText, string actionUrl, string newWindowUrl = null, string newWindowTitle = null, int newWindowWidth = 0, int newWindowMinHeight = 0)
        {
            return AutocompleteGridFor<TModel, AutocompleteRow, TId>(htmlHelper, expressionId, expressionText, actionUrl, lr => lr.Id, lr => lr.Value, null, newWindowUrl, newWindowTitle, newWindowWidth, newWindowMinHeight);
        }

        /// <summary>
        /// Lookup grid
        /// </summary>
        public static MvcHtmlString AutocompleteGridFor<TModel, TGridModel, TId>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TId>> expressionId, Expression<Func<TModel, string>> expressionText, string actionUrl, Expression<Func<TGridModel, object>> expressionLookupId, Expression<Func<TGridModel, object>> expressionLookupText, Expression<Func<TGridModel, object>>[] otherColumns = null, string newWindowUrl = null, string newWindowTitle = null, int newWindowWidth = 0, int newWindowMinHeight = 0)
        {
            if (((Object)expressionId) == null) throw new ArgumentNullException("expressionId");
            if (((Object)expressionText) == null) throw new ArgumentNullException("expressionText");
            if (String.IsNullOrEmpty(actionUrl)) throw new ArgumentException("The string cannot be null nor empty", "actionUrl");
            if (((Object)expressionLookupId) == null) throw new ArgumentNullException("expressionLookupId");
            if (((Object)expressionLookupText) == null) throw new ArgumentNullException("expressionLookupText");

            TagBuilder scriptTag = new TagBuilder("script");
            scriptTag.Attributes["type"] = "text/javascript";

            var idPropertyInfo = ExpressionHelper.GetPropertyInfoFromMemberExpression(expressionId);
            var textPropertyInfo = ExpressionHelper.GetPropertyInfoFromMemberExpression(expressionText);

            var htmlPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;

            var model = htmlHelper.ViewContext.ViewData.Model;
            var idPropertyValue = model != null ? idPropertyInfo.GetValue(model, null) : null;
            var textPropertyValue = model != null ? (string)textPropertyInfo.GetValue(model, null) : null;

            var inputHiddenId = string.IsNullOrEmpty(htmlPrefix) ? idPropertyInfo.Name : Encode(htmlPrefix + "." + idPropertyInfo.Name);
            var inputHiddenName = string.IsNullOrEmpty(htmlPrefix) ? idPropertyInfo.Name : (htmlPrefix + "." + idPropertyInfo.Name);
            var inputTextId = string.IsNullOrEmpty(htmlPrefix) ? textPropertyInfo.Name : Encode(htmlPrefix + "." + textPropertyInfo.Name);
            var inputTextName = string.IsNullOrEmpty(htmlPrefix) ? textPropertyInfo.Name : (htmlPrefix + "." + textPropertyInfo.Name);

            var options = new AutocompleteOptions()
            {
                contentUrl = actionUrl,
                inputHiddenId = inputHiddenId,
                inputHiddenName = inputHiddenName,
                inputHiddenValue = idPropertyValue,
                inputTextId = inputTextId,
                inputTextName = inputTextName,
                inputTextValue = textPropertyValue,
                newWindowUrl = newWindowUrl,
                newWindowWidth = newWindowWidth,
                newWindowMinHeight = newWindowMinHeight,
                newWindowTitle = newWindowTitle
            };

            var columnIdPropertyInfo = ExpressionHelper.GetPropertyInfoFromMemberExpression(expressionLookupId);
            options.columnId = columnIdPropertyInfo.Name;

            // coluna de texto
            var columnTextPropertyInfo = ExpressionHelper.GetPropertyInfoFromMemberExpression(expressionLookupText);
            options.columnText = columnTextPropertyInfo.Name;
            options.columns.Add(options.columnText);
            options.columnHeaders.Add(ExpressionHelper.GetDisplayName(expressionLookupText));

            // adiciona os nomes das colunas
            if (otherColumns != null)
                foreach (var columnExpression in otherColumns)
                {
                    var otherColumnPropertyInfo = ExpressionHelper.GetPropertyInfoFromMemberExpression(columnExpression);
                    options.columns.Add(otherColumnPropertyInfo.Name);
                    options.columnHeaders.Add(ExpressionHelper.GetDisplayName(columnExpression));
                }

            // validação das colunas
            if (!options.columns.Contains(options.columnText))
                throw new Exception(string.Format("O autocomplete possui configurações inválidas. "
                    + "A coluna de texto não faz parte da lista de colunas. Lookup: '{0}'. Coluna de text: '{1}'",
                    inputTextId, options.columnText));

            scriptTag.InnerHtml = string.Format("$(\"#{0}\").autocomplete({1});", inputTextId, new JavaScriptSerializer().Serialize(options));

            // renders

            var tagBuilder = new StringBuilder();

            var inputTextClasses = new List<string> { "autocomplete" };

            // determines if there's any validation issue
            if ((htmlHelper.ViewData.ModelState[inputTextName] != null && htmlHelper.ViewData.ModelState[inputTextName].Errors.Count > 0) ||
                (htmlHelper.ViewData.ModelState[inputHiddenName] != null && htmlHelper.ViewData.ModelState[inputHiddenName].Errors.Count > 0))
                inputTextClasses.Add("autocomplete-validation-error");

            tagBuilder.Append(htmlHelper.TextBoxFor(expressionText, new
            {
                @class = string.Join(" ", inputTextClasses.ToArray()),
                //autocomplete = "off" (this is done via script now, so that the browser restores the value of the input,
                // when hitting back button in chrome and firefox, and also when pressing F5 in firefox).
            }));
            tagBuilder.AppendLine();
            tagBuilder.Append(htmlHelper.HiddenFor(expressionId));
            tagBuilder.AppendLine();

            tagBuilder.Append(scriptTag.ToString());

            return new MvcHtmlString(tagBuilder.ToString());
        }

        /// <summary>
        /// Creates a collection editor for an N-Property
        /// </summary>
        private static MvcHtmlString CollectionEditorFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, ICollection>> expression, string listClass, string collectionItemEditor, string addAnotherText)
        {
            if (string.IsNullOrEmpty(collectionItemEditor)) throw new ArgumentException("collectionItemEditor cannot be null or empty");
            if (string.IsNullOrEmpty(addAnotherText)) throw new ArgumentException("addAnotherText cannot be null or empty");

            var propertyInfo = ExpressionHelper.GetPropertyInfoFromMemberExpression(expression);
            var addAnotherLinkId = "add-another-to-" + propertyInfo.Name.ToLower();
            var listCustomClass = propertyInfo.Name.ToLower() + "-list";

            var viewModel = new CollectionEditorViewModel()
            {
                ListParialViewName = collectionItemEditor,
                AddAnotherLinkId = addAnotherLinkId,
                ListClass = listClass,
                ListCustomClass = listCustomClass,
                Items = html.ViewContext.ViewData.Model != null ? new ArrayList(expression.Compile()((TModel)html.ViewContext.ViewData.Model)) : new ArrayList(),
                AddAnotherText = addAnotherText
            };

            return html.Partial("CollectionEditor", viewModel);
        }

        /// <summary>
        /// Creates a collection editor for an N-Property
        /// </summary>
        public static MvcHtmlString CollectionEditorFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, ICollection>> expression, string collectionItemEditor, string addAnotherText)
        {
            return CollectionEditorFor(html, expression, "edit-list", collectionItemEditor, addAnotherText);
        }


        /// <summary>
        /// Creates a collection editor for an N-Property
        /// </summary>
        public static MvcHtmlString CollectionEditorInlineFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, ICollection>> expression, string collectionItemEditor, string addAnotherText)
        {
            return CollectionEditorFor(html, expression, "edit-list-single-line", collectionItemEditor, addAnotherText);
        }

        /// <summary>
        /// Begins a collection item by inserting either a previously used .Index hidden field value for it or a new one.
        /// </summary>
        /// <param name="collectionName">The name of the collection property from the Model that owns this item.</param>
        public static IDisposable BeginCollectionItem<TModel>(this HtmlHelper<TModel> html, string collectionName)
        {
            if (String.IsNullOrEmpty(collectionName))
                throw new ArgumentException("collectionName is null or empty.", "collectionName");

            return new CollectionItemScope<TModel>(html, collectionName);
        }

        /// <summary>
        /// Begins a scope by inserting either a previously used .Index hidden field value for it or a new one.
        /// </summary>
        /// <param name="collectionName">The name of the scope from the Model that owns this item.</param>
        public static IDisposable BeginScope<TModel>(this HtmlHelper<TModel> html, string collectionName)
        {
            if (String.IsNullOrEmpty(collectionName))
                throw new ArgumentException("collectionName is null or empty.", "collectionName");

            return new ItemScope<TModel>(html, collectionName);
        }

        /// <summary>
        /// Begins a collection item by inserting either a previously used .Index hidden field value for it or a new one.
        /// </summary>
        /// <param name="collectionName">The name of the collection property from the Model that owns this item.</param>
        public static IDisposable BeginCollectionItemInline<TModel>(this HtmlHelper<TModel> html, string collectionName)
        {
            if (String.IsNullOrEmpty(collectionName))
                throw new ArgumentException("collectionName is null or empty.", "collectionName");

            return new CollectionItemInlineScope<TModel>(html, collectionName);
        }

        /// <summary>
        /// Tries to reuse old .Index values from the HttpRequest in order to keep the ModelState consistent
        /// across requests. If none are left returns a new one.
        /// </summary>
        /// <param name="collectionIndexFieldName"></param>
        /// <returns>a GUID string</returns>
        private static string GetCollectionItemIndex(string collectionIndexFieldName, HttpContextBase httpContext)
        {
            Queue<string> previousIndices = (Queue<string>)httpContext.Items[collectionIndexFieldName];
            if (previousIndices == null)
            {
                httpContext.Items[collectionIndexFieldName] = previousIndices = new Queue<string>();

                string previousIndicesValues = httpContext.Request[collectionIndexFieldName];
                if (!String.IsNullOrWhiteSpace(previousIndicesValues))
                {
                    foreach (string index in previousIndicesValues.Split(','))
                        previousIndices.Enqueue(index);
                }
            }

            return previousIndices.Count > 0 ? previousIndices.Dequeue() : Guid.NewGuid().ToString();
        }

        public abstract class CollectionItemScopeBase<TModel> : IDisposable
        {
            private readonly HtmlHelper<TModel> _html;
            private readonly string _previousPrefix;

            /// <summary>
            /// Writes the beggining of the collection item
            /// </summary>
            /// <param name="html"></param>
            public abstract void WriteBegin(HtmlHelper<TModel> html);

            /// <summary>
            /// Writes the end of the collection item
            /// </summary>
            /// <param name="html"></param>
            public abstract void WriteEnd(HtmlHelper<TModel> html);

            /// <summary>
            /// Starts the collection item scope
            /// </summary>
            protected CollectionItemScopeBase(HtmlHelper<TModel> html, string collectionName)
            {
                this.WriteBegin(html);

                var collectionIndexFieldName = String.Format("{0}.Index", collectionName);
                var itemIndex = GetCollectionItemIndex(collectionIndexFieldName, html.ViewContext.HttpContext);
                var collectionItemName = String.Format("{0}[{1}]", collectionName, itemIndex);

                var indexField = new TagBuilder("input");
                indexField.MergeAttributes(new Dictionary<string, string>() {
                    { "name", collectionIndexFieldName },
                    { "value", itemIndex },
                    { "type", "hidden" },
                    { "autocomplete", "off" }
                });

                html.ViewData.Add("collectionIndex", itemIndex);
                html.ViewContext.Writer.WriteLine(indexField.ToString(TagRenderMode.SelfClosing));

                this._previousPrefix = html.ViewData.TemplateInfo.HtmlFieldPrefix;
                html.ViewData.TemplateInfo.HtmlFieldPrefix = collectionItemName;

                this._html = html;
            }

            /// <summary>
            /// Finishes the collection item scope
            /// </summary>
            public void Dispose()
            {
                this._html.ViewData.TemplateInfo.HtmlFieldPrefix = this._previousPrefix;
                this.WriteEnd(this._html);
            }
        }

        public class ItemScope<TModel> : CollectionItemScopeBase<TModel>
        {
            public ItemScope(HtmlHelper<TModel> html, string collectionName) : base(html, collectionName) { }

            public override void WriteBegin(HtmlHelper<TModel> html)
            {
            }

            public override void WriteEnd(HtmlHelper<TModel> html)
            {
            }
        }

        public class CollectionItemScope<TModel> : CollectionItemScopeBase<TModel>
        {
            public CollectionItemScope(HtmlHelper<TModel> html, string collectionName) : base(html, collectionName) { }

            public override void WriteBegin(HtmlHelper<TModel> html)
            {
                html.ViewContext.Writer.WriteLine("<li class=\"edit-list-item\">");
                html.ViewContext.Writer.WriteLine("<div class=\"remove-button\" onclick=\"$(this).closest('.edit-list-item').remove()\"></div>");
                html.ViewContext.Writer.WriteLine("<div class=\"edit-list-item-wrapper\">");
            }

            public override void WriteEnd(HtmlHelper<TModel> html)
            {
                html.ViewContext.Writer.WriteLine("</div>");
                html.ViewContext.Writer.WriteLine("</li>");
            }
        }

        public class CollectionItemInlineScope<TModel> : CollectionItemScopeBase<TModel>
        {
            public CollectionItemInlineScope(HtmlHelper<TModel> html, string collectionName) : base(html, collectionName) { }

            public override void WriteBegin(HtmlHelper<TModel> html)
            {
                html.ViewContext.Writer.WriteLine("<li class=\"edit-list-item\">");
            }

            public override void WriteEnd(HtmlHelper<TModel> html)
            {
                html.ViewContext.Writer.WriteLine("<span onclick=\"$(this).closest('li').remove()\" class=\"close-collection-item\"></span>");
                html.ViewContext.Writer.WriteLine("</li>");
            }
        }
    }
}
