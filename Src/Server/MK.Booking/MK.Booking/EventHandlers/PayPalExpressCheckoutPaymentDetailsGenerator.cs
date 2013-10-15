using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PayPalExpressCheckoutPaymentDetailsGenerator :
        IEventHandler<PayPalExpressCheckoutPaymentInitiated>,
        IEventHandler<PayPalExpressCheckoutPaymentCancelled>,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PayPalExpressCheckoutPaymentDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public void Handle(PayPalExpressCheckoutPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var detail = new PayPalExpressCheckoutPaymentDetail
                                 {
                                     PaymentId = @event.SourceId,
                                     Amount = @event.Amount,
                                     OrderId = @event.OrderId,
                                     Token = @event.Token,
                                 };
                context.Save(detail);
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var detail = context.Set<PayPalExpressCheckoutPaymentDetail>().Find(@event.SourceId);
                if (detail == null)
                {
                    throw new InvalidOperationException("Payment not found");
                }
                detail.IsCancelled = true;
                context.SaveChanges();
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var detail = context.Set<PayPalExpressCheckoutPaymentDetail>().Find(@event.SourceId);
                if (detail == null)
                {
                    throw new InvalidOperationException("Payment not found");
                }
                detail.IsCompleted = true;
                detail.PayPalPayerId = @event.PayPalPayerId;
                
                detail.TransactionId = @event.TransactionId;
                context.SaveChanges();
            }
        }
    }
}