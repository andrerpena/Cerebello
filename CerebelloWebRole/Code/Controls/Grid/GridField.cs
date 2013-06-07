using System;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code
{
    public class GridField<TModel, TValue> : GridFieldBase
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }
    }
}
