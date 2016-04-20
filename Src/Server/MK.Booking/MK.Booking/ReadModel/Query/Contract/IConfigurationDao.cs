using System;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IConfigurationDao
    {
        ServerPaymentSettings GetPaymentSettings();

        NotificationSettings GetNotificationSettings(Guid? accountId = null);

        UserTaxiHailNetworkSettings GetUserTaxiHailNetworkSettings(Guid accountId);
    }
}