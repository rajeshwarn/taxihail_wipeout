using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IDeviceDao
    {
        IEnumerable<DeviceDetail> GetAll();
        IEnumerable<DeviceDetail> FindByAccountId(Guid AccountId);
    }
}