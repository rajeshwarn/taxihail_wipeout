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

        public MailSender(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            ICreditCardDao creditCardDao
            )
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _creditCardDao = creditCardDao;
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            if (@event.IsNoShowFee)
            {
                // Don't message user
                return;
            }

            SendReceiptIfNecessary(@event.OrderId);
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            SendReceiptIfNecessary(@event.OrderId);
        }
        
        private void SendReceiptIfNecessary(Guid orderId, bool sendOnlyIfPayInCar = false)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);

                // This is to prevent sending the receipt twice.
                if (order.Settings.ChargeTypeId != 1 && sendOnlyIfPayInCar)
                {
                    return;
                }

                var orderStatus = context.Find<OrderStatusDetail>(orderId);
                if (orderStatus != null)
                {
                    var orderPayment = context.Set<OrderPaymentDetail>().FirstOrDefault(p => p.OrderId == orderStatus.OrderId && p.IsCompleted );

                    var account = context.Find<AccountDetail>(orderStatus.AccountId);

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
                            orderPayment.SelectOrDefault(safe => Convert.ToDouble(safe.Meter), 0d),
                            0,
                            orderPayment.SelectOrDefault(safe => Convert.ToDouble(safe.Tip), 0d),
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
                SendReceiptIfNecessary(@event.SourceId, sendOnlyIfPayInCar: true );
            }
        }
    }
}
