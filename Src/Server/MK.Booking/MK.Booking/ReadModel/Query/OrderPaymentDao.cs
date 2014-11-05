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
                var orderPayments = context.Set<OrderPaymentDetail>()
                    .Where(x => x.OrderId == orderId);

                if (orderPayments.Count() <= 1)
                {
                    return orderPayments.FirstOrDefault();
                }

                // the only way an order could have 2 OrderPaymentDetail is because the user 
                // had his credit card preauthorized on order creation and paid with paypal
                // in this case, return the one with paypal
                var paypalPayment = orderPayments.FirstOrDefault(x => x.Provider == PaymentProvider.PayPal);
                return paypalPayment ?? orderPayments.FirstOrDefault();
            }
        }

        public OrderPaymentDetail FindNonPayPalByOrderId(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<OrderPaymentDetail>()
                    .FirstOrDefault(x => x.OrderId == orderId && x.Provider != PaymentProvider.PayPal);
            }
        }
        
        public OrderPaymentDetail FindByPayPalToken(string token)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<OrderPaymentDetail>()
                    .SingleOrDefault(x => x.PayPalToken == token);
            }
        }
    }
}