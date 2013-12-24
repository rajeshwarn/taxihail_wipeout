#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IDeviceDao
    {
        IEnumerable<DeviceDetail> GetAll();
        IEnumerable<DeviceDetail> FindByAccountId(Guid accountId);
    }
}