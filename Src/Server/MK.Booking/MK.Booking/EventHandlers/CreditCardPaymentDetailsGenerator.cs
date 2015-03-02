#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;
using RestSharp.Extensions;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardPaymentDetailsGenerator :
        IEventHandler<CreditCardPaymentInitiated>,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<CreditCardErrorThrown>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly Resources.Resources _resources;

        public CreditCardPaymentDetailsGenerator(Func<BookingDbContext> contextFactory, IServerSettings serverSettings)
        {
            _contextFactory = contextFactory;
            _resources = new Resources.Resources(serverSettings);
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Set<OrderPaymentDetail>().Find(@event.SourceId);
                if (payment == null)
                {
                    throw new InvalidOperationException("Payment not found");
                }

                payment.TransactionId = @event.TransactionId;
                payment.AuthorizationCode = @event.AuthorizationCode;
                payment.IsCompleted = true;
                payment.Amount = @event.Amount;
                payment.Meter = @event.Meter;
                payment.Tax = @event.Tax;
                payment.Tip = @event.Tip;

                // Update payment details after settling an overdue payment
                if (@event.NewCardToken.HasValue())
                {
                    payment.CardToken = @event.NewCardToken;
                }
                payment.IsCancelled = false;
                payment.Error = null;

                var order = context.Find<OrderDetail>(payment.OrderId);
                if (!order.Fare.HasValue || order.Fare == 0)
                {
                    order.Fare = Convert.ToDouble(@event.Meter);
                }
                if (!order.Tip.HasValue || order.Tip == 0)
                {
                    order.Tip = Convert.ToDouble(@event.Tip);
                }
                if (!order.Tax.HasValue || order.Tax == 0)
                {
                    order.Tax = Convert.ToDouble(@event.Tax);
                }

                var orderStatus = context.Find<OrderStatusDetail>(payment.OrderId);
                orderStatus.IBSStatusId = VehicleStatuses.Common.Done;
                orderStatus.IBSStatusDescription = _resources.Get("OrderStatus_wosDONE", order.ClientLanguageCode);

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
                    PreAuthorizedAmount = @event.Amount,
                    FirstPreAuthTransactionId = @event.TransactionId,
                    TransactionId = @event.TransactionId,
                    OrderId = @event.OrderId,
                    CardToken = @event.CardToken,
                    IsCompleted = false,
                    Provider = @event.Provider,
                    Type = @event.Provider == PaymentProvider.PayPal
                        ? PaymentType.PayPal
                        : PaymentType.CreditCard
                });
            }
        }

        public void Handle(CreditCardErrorThrown @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Set<OrderPaymentDetail>().Find(@event.SourceId);
                if (payment == null)
                {
                    throw new InvalidOperationException("Payment not found"); 
                }

                payment.IsCancelled = true;
                payment.Error = @event.Reason;

                context.Save(payment);
            }
        }
    }
}