using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;

namespace CerebelloWebRole.Code.Access
{
    public class FilteredObjectSetWrapper<TEntity> : FilteredObjectQueryWrapper<TEntity>, IObjectSet<TEntity>, IQueryable<TEntity>, IEnumerable<TEntity>, IQueryable, IEnumerable where TEntity : class
    {
        private readonly ObjectSet<TEntity> set;
        private readonly Func<ObjectQuery<TEntity>, IQueryable<TEntity>> filterFunc;
        private readonly IQueryable<TEntity> query;

        public FilteredObjectSetWrapper(ObjectSet<TEntity> set, Func<ObjectQuery<TEntity>, IQueryable<TEntity>> filterFunc)
            : base(set, filterFunc)
        {
            this.set = set;
            this.filterFunc = filterFunc;
            this.query = filterFunc(set);
        }

        public FilteredObjectQueryWrapper<TEntity> Include(string path)
        {
            return new FilteredObjectQueryWrapper<TEntity>(this.set.Include(path), this.filterFunc);
        }

        public void AddObject(TEntity entity)
        {
            this.set.AddObject(entity);
        }

        public void Attach(TEntity entity)
        {
            this.set.Attach(entity);
        }

        public void DeleteObject(TEntity entity)
        {
            this.set.DeleteObject(entity);
        }

        public void Detach(TEntity entity)
        {
            this.set.Detach(entity);
        }
    }
}
