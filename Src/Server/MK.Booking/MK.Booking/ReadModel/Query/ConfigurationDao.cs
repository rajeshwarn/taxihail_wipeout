#region

using System;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class ConfigurationDao : IConfigurationDao
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
                var settings = context.ServerPaymentSettings.Find(AppConstants.CompanyId);
                return settings ?? new ServerPaymentSettings();
            }
        }

        public NotificationSettings GetNotificationSettings(Guid? accountId = null)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (accountId.HasValue)
                {
                    return context.NotificationSettings.Find(accountId.Value);
                }

                return context.NotificationSettings.Find(AppConstants.CompanyId);
            }
        }
    }
}