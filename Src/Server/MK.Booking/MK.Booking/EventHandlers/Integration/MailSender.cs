﻿using System;
using System.Linq;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class MailSender : IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>,
          IEventHandler<OrderStatusChanged>
    {
        readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;

        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly ICreditCardDao _creditCardDao;

        public MailSender(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            IConfigurationManager configurationManager,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao
            )
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _configurationManager = configurationManager;
            _orderPaymentDao = orderPaymentDao;
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

                    var command = SendReceiptCommandBuilder.GetSendReceiptCommand(order, account,
                        orderStatus.VehicleNumber,
                        Convert.ToDouble(orderPayment.Meter), 0, Convert.ToDouble(orderPayment.Tip), 0, orderPayment,
                        card);


                    _commandBus.Send(command);

                }
            }

        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var sendDriverAssignedMail = _configurationManager.GetSetting("Booking.DriverAssignedConfirmationEmail", false);
                if (sendDriverAssignedMail && @event.Status.IBSStatusId == "wosASSIGNED")
                {
                    var order = context.Find<OrderDetail>(@event.SourceId);
                    var orderStatus = context.Find<OrderStatusDetail>(@event.SourceId);
                    var account = context.Find<AccountDetail>(@event.Status.AccountId);

                    var command = new SendAssignedConfirmation
                    {
                        Id = Guid.NewGuid(),
                        EmailAddress = account.Email,
                        IBSOrderId = order.IBSOrderId.GetValueOrDefault(),
                        PickupDate = order.PickupDate,
                        Fare = order.EstimatedFare.GetValueOrDefault(),
                        PickupAddress = order.PickupAddress,
                        DropOffAddress = order.DropOffAddress,
                        Settings = new SendBookingConfirmationEmail.BookingSettings
                            {
                                Name = account.Name,
                                Phone = account.Phone,
                                ChargeType = order.Settings.ChargeType,
                                Passengers = order.Settings.Passengers,
                                VehicleType = order.Settings.VehicleType
                            },
                        TransactionDate = order.CreatedDate,
                        VehicleNumber = orderStatus.VehicleNumber
                    };

                    _commandBus.Send(command);
                }
            }
        }


    }
}
