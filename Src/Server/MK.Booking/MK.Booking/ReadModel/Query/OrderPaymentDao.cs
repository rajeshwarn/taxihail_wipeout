using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

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

        public OrderPaymentDetail FindByOrderId(Guid orderId, string companyKey = null)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderPayments = context.Set<OrderPaymentDetail>().Where(x => x.OrderId == orderId);
                return orderPayments.Any()
                    ? orderPayments.FirstOrDefault(x => x.CompanyKey == companyKey)
                    : null;
            }
        }
    }
}