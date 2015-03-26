#region

using System;
using System.Linq;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class MailSender : IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>,
        IEventHandler<OrderStatusChanged>
    {
        private readonly ICommandBus _commandBus;
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;

        public MailSender(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            ICreditCardDao creditCardDao,
            IOrderDao orderDao)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _creditCardDao = creditCardDao;
            _orderDao = orderDao;
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            if (@event.IsNoShowFee)
            {
                // Don't message user
                return;
            }

            SendReceipt(@event.OrderId);
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            SendReceipt(@event.OrderId);
        }
        
        private void SendReceipt(Guid orderId, double? fare = null, double? tip = null)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = _orderDao.FindById(orderId);
                var orderStatus = _orderDao.FindOrderStatusById(orderId);

                if (orderStatus != null)
                {
                    var account = context.Find<AccountDetail>(orderStatus.AccountId);
                    var orderPayment = context.Set<OrderPaymentDetail>().FirstOrDefault(p => p.OrderId == orderStatus.OrderId && p.IsCompleted );
                    
                    CreditCardDetails card = null;
                    if (orderPayment != null && orderPayment.CardToken.HasValue())
                    {
                        card = _creditCardDao.FindByToken(orderPayment.CardToken);
                    }

                    var command = SendReceiptCommandBuilder.GetSendReceiptCommand(
                            order,
                            account,
                            orderStatus.VehicleNumber,
                            orderStatus.DriverInfos,
                            orderPayment.SelectOrDefault(payment => Convert.ToDouble(payment.Meter), fare),
                            0,
                            orderPayment.SelectOrDefault(payment => Convert.ToDouble(payment.Tip), tip),
                            0,
                            orderPayment,
                            card);

                    _commandBus.Send(command);
                }
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                var order = _orderDao.FindById(@event.SourceId);

                if (order.Settings.ChargeTypeId == ChargeTypes.PaymentInCar.Id)
                {
                    SendReceipt(@event.SourceId, @event.Fare, @event.Tip);
                }
            }
        }
    }
}
