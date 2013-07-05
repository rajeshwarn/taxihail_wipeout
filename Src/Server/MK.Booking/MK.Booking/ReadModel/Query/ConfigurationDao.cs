using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.ReadModel
{
    class ConfigurationDao : IConfigurationDao
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;
        private readonly IConfigurationManager _configurationManager;

        public ConfigurationDao(Func<ConfigurationDbContext> contextFactory, IConfigurationManager configurationManager)
        {
            _contextFactory = contextFactory;
            _configurationManager = configurationManager;
        }

        public ServerPaymentSettings GetPaymentSettings()
        {
            using (var context = _contextFactory.Invoke())
            {
                var settings = context.ServerPaymentSettings.SingleOrDefault();
                if (settings != null)
                {
                    var ppSettings = context.Query<PayPalServerSettings>().SingleOrDefault();
                    settings.PayPalServerSettings = ppSettings;
                    return settings;
                }
                return new ServerPaymentSettings(Guid.NewGuid());
            }
        }
    
    }
}
