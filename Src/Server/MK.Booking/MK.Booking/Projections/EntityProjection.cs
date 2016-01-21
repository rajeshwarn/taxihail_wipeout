using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace apcurium.MK.Booking.Projections
{
    public class EntityProjection<TProjection> : IProjection<TProjection> where TProjection : class
    {
        readonly Func<DbContext> _contextFactory;
        private readonly Func<TProjection, bool> _predicate;
        readonly object[] _keyValues;

        public EntityProjection(Func<DbContext> contextFactory, Func<TProjection, bool> predicate, params object[] keyValues)
        {
            _keyValues = keyValues;
            _contextFactory = contextFactory;
            _predicate = predicate;
        }

        public TProjection Load()
        {
            using (var context = _contextFactory.Invoke())
            {
                return _predicate != null 
                    ? context.Set<TProjection>().FirstOrDefault(_predicate) 
                    : context.Set<TProjection>().Find(_keyValues);
            }
        }

        public void Save(TProjection projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<TProjection>().AddOrUpdate(projection);
                context.SaveChanges();
            }
        }
    }
}
