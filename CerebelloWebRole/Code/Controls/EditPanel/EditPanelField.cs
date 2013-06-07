using System;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Campo do EditPanel
    /// </summary>
    public class EditPanelField<TModel, TValue> : EditPanelFieldBase
    {

// ReSharper disable MemberCanBePrivate.Global
        // this member is accessed via reflaction and being public makes it easier
        public Expression<Func<TModel, TValue>> Expression { get; set; }
// ReSharper restore MemberCanBePrivate.Global

        public EditPanelField(Expression<Func<TModel, TValue>> expression, EditPanelFieldSize size = EditPanelFieldSize.Default, Func<dynamic, object> editorFormat = null, string header = null, bool foreverAlone = false)
        {
            this.Format = editorFormat;
            this.Expression = expression;
            this.Header = header;
            this.WholeRow = foreverAlone;
            this.Size = size;
        }
    }
}
