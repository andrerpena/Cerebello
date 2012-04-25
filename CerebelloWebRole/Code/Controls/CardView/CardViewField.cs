using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

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
    }
}
