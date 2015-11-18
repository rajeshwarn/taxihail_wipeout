using EntityFramework.BulkInsert.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Projections
{
    public class EntityProjectionSet<TProjection> : IProjectionSet<TProjection> where TProjection : class
    {
        private readonly Func<DbContext> _contextFactory;
        public EntityProjectionSet(Func<DbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Add(TProjection projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<TProjection>().Add(projection);
                context.SaveChanges();
            }
        }

        public void AddOrReplace(TProjection projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<TProjection>().AddOrUpdate(projection);
                context.SaveChanges();
            }
        }

        public void AddRange(IEnumerable<TProjection> projections)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.BulkInsert(projections, new BulkInsertOptions
                {
                    EnableStreaming = true,
                });
            }
        }

        public bool Exists(Guid identifier)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<TProjection>().Find(identifier) != null;
            }
        }

        public void Update(Guid identifier, Action<TProjection> action)
        {
            using (var context = _contextFactory.Invoke())
            {
                var projection = context.Set<TProjection>().Find(identifier);
                if (projection == null)
                {
                    throw new InvalidOperationException("Projection not found");
                }
                action.Invoke(projection);
                context.SaveChanges();
            }
        }

        public void Update(Func<TProjection, bool> predicate, Action<TProjection> action)
        {
            using (var context = _contextFactory.Invoke())
            {
                var projections = context.Set<TProjection>().Where(predicate);
                foreach (var projection in projections)
                {
                    action.Invoke(projection);
                }
                context.SaveChanges();
            }
        }

        public IProjection<TProjection> GetProjection(Guid identifier)
        {
            return new EntityProjection<TProjection>(_contextFactory, identifier);
        }

    }
}
