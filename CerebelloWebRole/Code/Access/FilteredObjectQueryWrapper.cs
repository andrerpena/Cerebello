using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code
{
    public class FilteredObjectQueryWrapper<TEntity> : IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable where TEntity : class
    {
        private readonly ObjectQuery<TEntity> set;
        private readonly Func<ObjectQuery<TEntity>, IQueryable<TEntity>> filterFunc;
        private readonly IQueryable<TEntity> query;

        public FilteredObjectQueryWrapper(ObjectQuery<TEntity> set, Func<ObjectQuery<TEntity>, IQueryable<TEntity>> filterFunc)
        {
            this.set = set;
            this.filterFunc = filterFunc;
            this.query = filterFunc(set);
        }

        public FilteredObjectQueryWrapper<TEntity> Include(string path)
        {
            return new FilteredObjectQueryWrapper<TEntity>(this.set.Include(path), this.filterFunc);
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return this.query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.query.GetEnumerator();
        }

        Type IQueryable.ElementType
        {
            get { return this.query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return this.query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.query.Provider; }
        }
    }
}
