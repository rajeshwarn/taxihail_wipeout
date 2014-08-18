#region

using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class PayPalExpressCheckoutPaymentDetailsGenerator :
        IEventHandler<PayPalExpressCheckoutPaymentInitiated>,
        IEventHandler<PayPalExpressCheckoutPaymentCancelled>,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<PayPalPaymentCancellationFailed>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PayPalExpressCheckoutPaymentDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public void Handle(PayPalExpressCheckoutPaymentCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var detail = context.Set<OrderPaymentDetail>().Find(@event.SourceId);
                if ((detail == null) || (detail.Type != PaymentType.PayPal))
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
                var detail = context.Set<OrderPaymentDetail>().Find(@event.SourceId);
                if ((detail == null) || (detail.Type != PaymentType.PayPal))
                {
                    throw new InvalidOperationException("Payment not found");
                }
                detail.IsCompleted = true;
                detail.PayPalPayerId = @event.PayPalPayerId;
                detail.AuthorizationCode = @event.PayPalPayerId;
                detail.TransactionId = @event.TransactionId;
                context.SaveChanges();
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var detail = new OrderPaymentDetail
                {
                    PaymentId = @event.SourceId,
                    Amount = @event.Amount,
                    Meter = @event.Meter,
                    Tip = @event.Tip,
                    OrderId = @event.OrderId,
                    PayPalToken = @event.Token,
                    Provider = PaymentProvider.PayPal,
                    Type = PaymentType.PayPal
                };
                context.Save(detail);
            }
        }

        public void Handle(PayPalPaymentCancellationFailed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var detail = context.Set<OrderPaymentDetail>().Find(@event.SourceId);
                if ((detail == null) || (detail.Type != PaymentType.PayPal))
                {
                    throw new InvalidOperationException("Payment not found");
                }
                detail.Error = @event.Reason;
                context.SaveChanges();
            }
        }
    }
}