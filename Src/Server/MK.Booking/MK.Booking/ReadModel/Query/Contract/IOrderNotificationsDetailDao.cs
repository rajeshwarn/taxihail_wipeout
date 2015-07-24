using apcurium.MK.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOrderNotificationsDetailDao
    {
        OrderNotificationDetail FindByOrderId(Guid orderId);
    }
}