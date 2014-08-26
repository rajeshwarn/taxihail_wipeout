#region

using System;
using apcurium.MK.Common.Configuration.Impl;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IConfigurationDao
    {
        ServerPaymentSettings GetPaymentSettings();
        NotificationSettings GetNotificationSettings(Guid? accountId = null);
    }
}