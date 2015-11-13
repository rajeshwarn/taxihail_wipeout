using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Projections
{
    public class EntityProjection<TProjection> : IProjection<TProjection> where TProjection : class
    {
        readonly Func<DbContext> _contextFactory;
        readonly object[] _keyValues;
        public EntityProjection(Func<DbContext> contextFactory, params object[] keyValues)
        {
            _keyValues = keyValues;
            _contextFactory = contextFactory;
        }

        public TProjection Load()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<TProjection>().Find(_keyValues);
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
