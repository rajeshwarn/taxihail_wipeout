using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Projections
{
    public abstract class AppSettingsProjection : IProjection<IDictionary<string, string>>
    {
        public abstract IDictionary<string, string> Load();

        public abstract void Save(IDictionary<string, string> projection);
    }

    public class AppSettingsEntityProjection : AppSettingsProjection
    {
        private readonly Func<DbContext> _contextFactory;
        public AppSettingsEntityProjection(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override IDictionary<string, string> Load()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<AppSetting>().ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public override void Save(IDictionary<string, string> projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                var keys = projection.Keys.ToArray();
                context.Set<AppSetting>().RemoveRange(context.Set<AppSetting>().Where(x => !keys.Contains(x.Key)));
                context.Set<AppSetting>().AddOrUpdate(projection.Select(x => new AppSetting { Key = x.Key, Value = x.Value }).ToArray());
                context.SaveChanges();
            }
        }
    }

    public class AppSettingsMemoryProjection : AppSettingsProjection
    {
        IDictionary<string, string> _dictionnary = new Dictionary<string, string>();

        public override IDictionary<string, string> Load()
        {
            return new Dictionary<string, string>(_dictionnary);
        }

        public override void Save(IDictionary<string, string> projection)
        {
            _dictionnary = projection;
        }
    }
}