using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code.Controls
{
    public class GridField<TModel, TValue> : GridFieldBase
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }

        public GridField(Expression<Func<TModel, TValue>> exp, Func<dynamic, object> format = null, string header = null, bool canSort = false, bool wordWrap = false)
        {
            this.Format = format;
            this.Expression = exp;
            this.Header = header;
            this.CanSort = CanSort;
            this.WordWrap = wordWrap;
        }
    }
}
