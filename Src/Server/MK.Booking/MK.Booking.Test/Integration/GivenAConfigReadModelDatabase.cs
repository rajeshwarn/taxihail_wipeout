using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Test.Integration
{
    public class given_a_config_read_model_database : IDisposable
    {
        protected string dbName;

        static given_a_config_read_model_database()
        {
            new Booking.Module().RegisterMaps();
        }

        public given_a_config_read_model_database()
        {
            dbName = this.GetType().Name + "-" + Guid.NewGuid().ToString();
            using (var context = new ConfigurationDbContext(dbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }
        }

        public void Dispose()
        {
            using (var context = new ConfigurationDbContext(dbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();
            }
        }
    }
}
