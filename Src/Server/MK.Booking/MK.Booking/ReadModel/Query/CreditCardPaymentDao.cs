using System;
using System.Linq;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class CreditCardPaymentDao: ICreditCardPaymentDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public CreditCardPaymentDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public CreditCardPaymentDetail FindByTransactionId(string transactionId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<CreditCardPaymentDetail>()
                    .SingleOrDefault(x => x.TransactionId == transactionId);
            }
        }

        public CreditCardPaymentDetail FindByOrderId(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<CreditCardPaymentDetail>()
                    .SingleOrDefault(x => x.OrderId == orderId);
            }
        }
    }
}
