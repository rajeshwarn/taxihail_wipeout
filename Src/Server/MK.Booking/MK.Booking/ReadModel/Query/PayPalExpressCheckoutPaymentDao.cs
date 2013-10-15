using System;
using System.Linq;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class PayPalExpressCheckoutPaymentDao: IPayPalExpressCheckoutPaymentDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PayPalExpressCheckoutPaymentDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public PayPalExpressCheckoutPaymentDetail FindByToken(string token)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<PayPalExpressCheckoutPaymentDetail>().SingleOrDefault(x => x.Token == token);
            }
        }

        public PayPalExpressCheckoutPaymentDetail FindByOrderId(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
#if DEBUG
                return context.Set<PayPalExpressCheckoutPaymentDetail>().SingleOrDefault(x => x.OrderId == orderId);
#else
                return context.Set<PayPalExpressCheckoutPaymentDetail>().FirstOrDefault(x => x.OrderId == orderId);
#endif
            }
        }
    }
}
