using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.ReadModel
{
    class ConfigurationDao : IConfigurationDao
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public ConfigurationDao(Func<ConfigurationDbContext> contextFactory)
        {            
            _contextFactory = contextFactory;
        }

        public ServerPaymentSettings GetPaymentSettings()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<ServerPaymentSettings>().SingleOrDefault();
            }
        }
    
    }
}
