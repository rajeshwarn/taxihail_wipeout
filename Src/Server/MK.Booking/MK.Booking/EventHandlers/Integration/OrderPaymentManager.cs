﻿using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager :
        IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>,
        IEventHandler<OrderCancelled>
    {
        private readonly IOrderDao _dao;
        private readonly IIbsOrderService _ibs;
        private readonly IServerSettings _serverSettings;
        private readonly IPaymentService _paymentService;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;

        public OrderPaymentManager(IOrderDao dao, IOrderPaymentDao paymentDao, IAccountDao accountDao, 
            ICreditCardDao creditCardDao, IIbsOrderService ibs, IServerSettings serverSettings, IPaymentService paymentService)
        {
            _accountDao = accountDao;
            _dao = dao;
            _paymentDao = paymentDao;
            _creditCardDao = creditCardDao;
            _ibs = ibs;
            _serverSettings = serverSettings;
            _paymentService = paymentService;
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            // Send message to driver
            SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, @event.Meter, @event.Tip, PaymentProvider.PayPal.ToString(), @event.PayPalPayerId);
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            if (@event.IsNoShowFee)
            {
                // Don't message driver
                return;
            }

            if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
            {
                SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, @event.Meter, @event.Tip, @event.Provider.ToString(), @event.AuthorizationCode);
            }
        }

        private void SendPaymentConfirmationToDriver(Guid orderId, decimal amount, decimal meter, decimal tip, string provider,  string authorizationCode)
        {
            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(orderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(orderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");
            if (orderDetail.IBSOrderId == null) throw new InvalidOperationException("IBSOrderId should not be null");

            var payment = _paymentDao.FindByOrderId(orderId);
            if (payment == null) throw new InvalidOperationException("Payment info not found");

            var account = _accountDao.FindById(orderDetail.AccountId);
            if (account == null) throw new InvalidOperationException("Order account not found");

            if ( provider == PaymentType.CreditCard.ToString () )
            {
                var card = _creditCardDao.FindByToken(payment.CardToken);
                if (card == null) throw new InvalidOperationException("Credit card not found");
            }

            _ibs.SendPaymentNotification((double)amount, (double)meter, (double)tip, authorizationCode, orderStatusDetail.VehicleNumber);
        }

        public void Handle(OrderCancelled @event)
        {
            // void the preauthorization to prevent misuse fees
            _paymentService.VoidPreAuthorization(@event.SourceId);
        }
    }
}