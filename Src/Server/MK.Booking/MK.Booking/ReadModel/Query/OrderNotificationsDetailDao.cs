using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class OrderNotificationsDetailDao:IOrderNotificationsDetailDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public OrderNotificationsDetailDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public OrderNotificationDetail FindByOrderId(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<OrderNotificationDetail>()
                    .FirstOrDefault(x => x.Id == orderId);
            }
        }
    }
}