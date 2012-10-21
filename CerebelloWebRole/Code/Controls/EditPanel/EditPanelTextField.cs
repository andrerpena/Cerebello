using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace CerebelloWebRole.Code.Controls
{
    public class EditPanelTextField<TModel, TValue> : EditPanelFieldBase
    {
// ReSharper disable MemberCanBePrivate.Global
        // this member is accessed via reflaction and being public makes it easier
        public Expression<Func<TModel, TValue>> Expression { get; set; }
// ReSharper restore MemberCanBePrivate.Global

        public EditPanelTextField(Expression<Func<TModel, TValue>> exp, Func<dynamic, object> format = null, Func<dynamic, object> formatDescription = null, string header = null, bool foreverAlone = false)
        {
            this.Format = format;
            this.FormatDescription = formatDescription;
            this.Expression = exp;
            this.Header = header;
            this.WholeRow = foreverAlone;
        }
    }
}