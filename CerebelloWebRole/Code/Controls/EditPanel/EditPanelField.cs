using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code.Controls
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

        public EditPanelField(Expression<Func<TModel, TValue>> exp, EditPanelFieldSize size = EditPanelFieldSize.Default, Func<dynamic, object> format = null, Func<dynamic, object> formatDescription = null, string header = null, bool foreverAlone = false)
        {
            this.Format = format;
            this.FormatDescription = formatDescription;
            this.Expression = exp;
            this.Header = header;
            this.WholeRow = foreverAlone;
            this.Size = size;
        }
    }
}
