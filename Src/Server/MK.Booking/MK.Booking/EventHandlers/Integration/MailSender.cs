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
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class MailSender : IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>
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
            SendReceipt(@event.OrderId);
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            SendReceipt(@event.OrderId);
        }
        
        private void SendReceipt(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);
                var orderStatus = context.Find<OrderStatusDetail>(orderId);
                if (orderStatus != null)
                {
                    var orderPayment =
                        context.Set<OrderPaymentDetail>().SingleOrDefault(p => p.OrderId == orderStatus.OrderId);
                    var account = context.Find<AccountDetail>(orderStatus.AccountId);

                    CreditCardDetails card = null;
                    if ((orderPayment != null) && (orderPayment.CardToken.HasValue()))
                    {
                        card = _creditCardDao.FindByToken(orderPayment.CardToken);
                    }

                    if (orderPayment != null)
                    {
                        var command = SendReceiptCommandBuilder.GetSendReceiptCommand(order, account,
                            orderStatus.VehicleNumber, orderStatus.DriverInfos.FullName,
                            Convert.ToDouble(orderPayment.Meter), 0, Convert.ToDouble(orderPayment.Tip), 0, orderPayment,
                            card);
                        
                        _commandBus.Send(command);
                    }
                }
            }
        }
    }
}
