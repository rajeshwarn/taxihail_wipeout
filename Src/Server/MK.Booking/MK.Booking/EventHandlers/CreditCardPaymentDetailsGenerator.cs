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
using apcurium.MK.Booking.Projections;

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardPaymentDetailsGenerator :
        IEventHandler<CreditCardPaymentInitiated>,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<CreditCardErrorThrown>,
        IEventHandler<PrepaidOrderPaymentInfoUpdated>,
        IEventHandler<RefundedOrderUpdated>
    {
        private readonly IProjectionSet<OrderDetail> _orderDetailProjectionSet;
        private readonly IProjectionSet<OrderStatusDetail> _orderStatusProjectionSet;
        private readonly IProjectionSet<OrderPaymentDetail> _orderPaymentProjectionSet;
        private readonly Resources.Resources _resources;

        public CreditCardPaymentDetailsGenerator(
            IProjectionSet<OrderDetail> orderDetailProjectionSet,
            IProjectionSet<OrderStatusDetail> orderStatusProjectionSet,
            IProjectionSet<OrderPaymentDetail> orderPaymentProjectionSet,
            IServerSettings serverSettings)
        {
            _orderDetailProjectionSet = orderDetailProjectionSet;
            _orderStatusProjectionSet = orderStatusProjectionSet;
            _orderPaymentProjectionSet = orderPaymentProjectionSet;

            _resources = new Resources.Resources(serverSettings);
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            @event.MigrateFees();

            var payment = _orderPaymentProjectionSet.GetProjection(@event.SourceId).Load();
            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found");
            }

            _orderPaymentProjectionSet.Update(@event.SourceId, orderPaymentDetail =>
            {
                orderPaymentDetail.TransactionId = @event.TransactionId;
                orderPaymentDetail.AuthorizationCode = @event.AuthorizationCode;
                orderPaymentDetail.IsCompleted = true;
                orderPaymentDetail.Amount = @event.Amount;
                orderPaymentDetail.Meter = @event.Meter;
                orderPaymentDetail.Tax = @event.Tax;
                orderPaymentDetail.Tip = @event.Tip;
                orderPaymentDetail.Toll = @event.Toll;
                orderPaymentDetail.Surcharge = @event.Surcharge;
                orderPaymentDetail.BookingFees = @event.BookingFees;
                orderPaymentDetail.IsCancelled = false;
                orderPaymentDetail.FeeType = @event.FeeType;
                orderPaymentDetail.Error = null;

                // Update payment details after settling an overdue payment
                if (@event.NewCardToken.HasValue())
                {
                    orderPaymentDetail.CardToken = @event.NewCardToken;
                }
            });

            var orderExists = _orderDetailProjectionSet.Exists(payment.OrderId);

            // Prevents NullReferenceException caused with web prepayed while running database initializer.
            if (!orderExists && @event.IsForPrepaidOrder)
            {
                _orderDetailProjectionSet.Add(new OrderDetail
                {
                    Id = payment.OrderId,
                    //Following values will be set to the correct date and time when that event is played.
                    PickupDate = @event.EventDate,
                    CreatedDate = @event.EventDate
                });
            }

            var clientLanguageCode = "en";
            _orderDetailProjectionSet.Update(payment.OrderId, order =>
            {
                clientLanguageCode = order.ClientLanguageCode;

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
            }); 

            if (!@event.IsForPrepaidOrder)
            {
                _orderStatusProjectionSet.Update(payment.OrderId, orderStatus =>
                {
                    orderStatus.IBSStatusId = VehicleStatuses.Common.Done;
                    orderStatus.IBSStatusDescription = _resources.Get("OrderStatus_wosDONE", clientLanguageCode);
                });   
            }
        }

        public void Handle(CreditCardPaymentInitiated @event)
        {
            _orderPaymentProjectionSet.Add(new OrderPaymentDetail
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

        public void Handle(CreditCardErrorThrown @event)
        {
            if (!_orderPaymentProjectionSet.Exists(@event.SourceId))
            {
                throw new InvalidOperationException("Payment not found");
            }

            _orderPaymentProjectionSet.Update(@event.SourceId, payment =>
            {
                payment.IsCancelled = true;
                payment.Error = @event.Reason;
            });
        }

        public void Handle(PrepaidOrderPaymentInfoUpdated @event)
        {
            _orderPaymentProjectionSet.Add(new OrderPaymentDetail
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

        public void Handle(RefundedOrderUpdated @event)
        {
            if (_orderPaymentProjectionSet.Exists(x => x.OrderId == @event.SourceId))
            {
                _orderPaymentProjectionSet.Update(x => x.OrderId == @event.SourceId, payment =>
                {
                    payment.IsRefunded = @event.IsSuccessful;
                    payment.Error = @event.Message;
                });
            }
        }
    }
}