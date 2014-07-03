#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOrderStatusUpdateDao
    {
        OrderStatusUpdateDetail GetLastUpdate();
        void UpdateLastUpdate(string updaterUniqueId, DateTime updateTime );
    }
}