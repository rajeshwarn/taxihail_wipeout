using System;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.ReadModel
{
    internal class ConfigurationDao : IConfigurationDao
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public ConfigurationDao(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public ServerPaymentSettings GetPaymentSettings()
        {
            using (ConfigurationDbContext context = _contextFactory.Invoke())
            {
                ServerPaymentSettings settings = context.ServerPaymentSettings.Find(AppConstants.CompanyId);
                return settings ?? new ServerPaymentSettings();
            }
        }
    }
}