#region

using System;
using apcurium.MK.Common.Configuration.Impl;

#endregion

namespace apcurium.MK.Booking.Test.Integration
{
// ReSharper disable once InconsistentNaming
    public class given_a_config_read_model_database : IDisposable
    {
        protected string DbName;

        static given_a_config_read_model_database()
        {
            new Module().RegisterMaps();
        }

        public given_a_config_read_model_database()
        {
            DbName = GetType().Name + "-" + Guid.NewGuid();
            using (var context = new ConfigurationDbContext(DbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }
        }

        public void Dispose()
        {
            using (var context = new ConfigurationDbContext(DbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();
            }
        }
    }
}