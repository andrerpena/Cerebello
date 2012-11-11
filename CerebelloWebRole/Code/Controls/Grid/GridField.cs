using System;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code.Controls
{
    public class GridField<TModel, TValue> : GridFieldBase
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }
    }
}
