#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class OrderPaymentDao : IOrderPaymentDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public OrderPaymentDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public OrderPaymentDetail FindByTransactionId(string transactionId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<OrderPaymentDetail>()
                    .SingleOrDefault(x => x.TransactionId == transactionId);
            }
        }

        public OrderPaymentDetail FindByOrderId(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<OrderPaymentDetail>()
                    .FirstOrDefault(x => x.OrderId == orderId);
            }
        }
    }
}