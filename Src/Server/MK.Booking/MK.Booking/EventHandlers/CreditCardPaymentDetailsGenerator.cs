using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardPaymentDetailsGenerator :
        IEventHandler<CreditCardPaymentInitiated>,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<CreditCardErrorThrown>,
        IEventHandler<PrepaidOrderPaymentInfoUpdated>,
        IEventHandler<RefundedOrderUpdated>
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
            @event.MigrateFees();

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
                payment.Toll = @event.Toll;
                payment.Surcharge = @event.Surcharge;
                payment.BookingFees = @event.BookingFees;
                payment.IsCancelled = false;
                payment.FeeType = @event.FeeType;
                payment.Error = null;

                // Update payment details after settling an overdue payment
                if (@event.NewCardToken.HasValue())
                {
                    payment.CardToken = @event.NewCardToken;
                }

                var order = context.Find<OrderDetail>(payment.OrderId);

                // Prevents NullReferenceException caused with web prepayed while running database initializer.
                if (order == null && @event.IsForPrepaidOrder)
                {
                    order = new OrderDetail
                    {
                        Id = payment.OrderId,
                        //Following values will be set to the correct date and time when that event is played.
                        PickupDate = @event.EventDate,
                        CreatedDate = @event.EventDate
                    };

                    context.Set<OrderDetail>().Add(order);
                }


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
                if (!order.Toll.HasValue || order.Toll == 0)
                {
                    order.Toll = Convert.ToDouble(@event.Toll);
                }
                if (!order.Surcharge.HasValue || order.Surcharge == 0)
                {
                    order.Surcharge = Convert.ToDouble(@event.Surcharge);
                }

                if (!@event.IsForPrepaidOrder)
                {
                    var orderStatus = context.Find<OrderStatusDetail>(payment.OrderId);
                    orderStatus.IBSStatusId = VehicleStatuses.Common.Done;
                    orderStatus.IBSStatusDescription = _resources.Get("OrderStatus_wosDONE", order.ClientLanguageCode);
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
                    PreAuthorizedAmount = @event.Amount,
                    FirstPreAuthTransactionId = @event.TransactionId,
                    TransactionId = @event.TransactionId,
                    OrderId = @event.OrderId,
                    CardToken = @event.CardToken,
                    IsCompleted = false,
                    Provider = @event.Provider,
                    Type = @event.Provider == PaymentProvider.PayPal
                        ? PaymentType.PayPal
                        : PaymentType.CreditCard,
                    CompanyKey = @event.CompanyKey
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

        public void Handle(PrepaidOrderPaymentInfoUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new OrderPaymentDetail
                {
                    PaymentId = @event.SourceId,
                    Amount = @event.Amount,
                    Meter = @event.Meter,
                    Tax = @event.Tax,
                    Tip = @event.Tip,
                    OrderId = @event.OrderId,
                    TransactionId = @event.TransactionId,
                    Provider = PaymentProvider.PayPal,
                    Type = PaymentType.PayPal,
                    IsCompleted = true
                });
            }
        }

        public void Handle(RefundedOrderUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Set<OrderPaymentDetail>().FirstOrDefault(p => p.OrderId == @event.SourceId);
                if (payment != null)
                {
                    payment.IsRefunded = @event.IsSuccessful;
                    payment.Error = @event.Message;

                    context.Save(payment);
                }

                var order = context.Set<OrderDetail>().FirstOrDefault(o => o.Id == @event.SourceId);
                if(order != null)
                {
                    order.IsRefunded = @event.IsSuccessful;
                    context.Save(order);
                }
            }
        }
    }
}