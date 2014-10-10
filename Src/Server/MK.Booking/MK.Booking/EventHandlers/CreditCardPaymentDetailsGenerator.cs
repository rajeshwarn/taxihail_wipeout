﻿#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardPaymentDetailsGenerator :
        IEventHandler<CreditCardPaymentInitiated>,
        IEventHandler<CreditCardPaymentCaptured>,
        IEventHandler<CreditCardErrorThrown>

    {
        private readonly Func<BookingDbContext> _contextFactory;

        public CreditCardPaymentDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Set<OrderPaymentDetail>().Find(@event.SourceId);
                if (payment == null) throw new InvalidOperationException("Payment not found");
                payment.AuthorizationCode = @event.AuthorizationCode;
                payment.IsCompleted = true;

                var order = context.Set<OrderDetail>().Single(o => o.Id == payment.OrderId);
                if (!order.Fare.HasValue || order.Fare == 0)
                {
                    order.Fare = Convert.ToDouble(payment.Meter);
                }

                if (!order.Tip.HasValue || order.Tip == 0)
                {
                    order.Tip = Convert.ToDouble(payment.Tip);
                }

                if (payment.Amount == 0 && payment.Amount == 0 && payment.Tip == 0)
                {
                    // Replace the PreAuth values with the real amounts
                    payment.Amount = @event.Amount;
                    payment.Meter = @event.Meter;
                    payment.Tip = @event.Tip;
                }

                context.SaveChanges();
            }
        }

        public void Handle(CreditCardPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new OrderPaymentDetail
                {
                    PaymentId = @event.SourceId,
                    Amount = @event.Amount,
                    Meter = @event.Meter,
                    Tip = @event.Tip,
                    TransactionId = @event.TransactionId,
                    OrderId = @event.OrderId,
                    CardToken = @event.CardToken,
                    IsCompleted = false,
                    Provider = @event.Provider,
                    Type = PaymentType.CreditCard,
                });
            }
        }

        public void Handle(CreditCardErrorThrown @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Set<OrderPaymentDetail>().Find(@event.SourceId);
                if (payment == null) throw new InvalidOperationException("Payment not found");
                payment.Error = @event.Reason;
                context.SaveChanges();
            }
        }
    }
}