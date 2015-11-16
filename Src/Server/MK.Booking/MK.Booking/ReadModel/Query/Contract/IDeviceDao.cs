#region

using apcurium.MK.Common.Enumeration;
using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IDeviceDao
    {
        IEnumerable<DeviceDetail> GetAll();
        IEnumerable<DeviceDetail> FindByAccountId(Guid accountId);
        void Add(Guid accountId, string deviceToken, PushNotificationServicePlatform platform);
        void Remove(Guid accountId, string deviceToken);
    }
}