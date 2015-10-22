using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager :
        IIntegrationEventHandler,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderSwitchedToNextDispatchCompany>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderCancelledBecauseOfError>
    {
        private readonly IOrderDao _dao;
        private readonly IIbsOrderService _ibs;
        private readonly IServerSettings _serverSettings;
        private readonly IPaymentService _paymentService;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;

        public OrderPaymentManager(IOrderDao dao, IOrderPaymentDao paymentDao, IAccountDao accountDao, IOrderDao orderDao, ICommandBus commandBus,
            ICreditCardDao creditCardDao, IIbsOrderService ibs, IServerSettings serverSettings, IPaymentService paymentService)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _commandBus = commandBus;
            _dao = dao;
            _paymentDao = paymentDao;
            _creditCardDao = creditCardDao;
            _ibs = ibs;
            _serverSettings = serverSettings;
            _paymentService = paymentService;
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            if (@event.IsNoShowFee)
            {
                // Don't message driver
                return;
            }

            if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver
                && !@event.IsSettlingOverduePayment) // Don't send notification to driver when user settles overdue payment
            {
                // To prevent driver confusion we will not send the discounted total amount for the fare.
                SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount + @event.AmountSavedByPromotion, @event.Meter + @event.Tax, @event.Tip, @event.Provider.ToString(), @event.AuthorizationCode);
            }

            if (@event.PromotionUsed.HasValue)
            {
                var redeemPromotion = new RedeemPromotion
                {
                    OrderId = @event.OrderId,
                    PromoId = @event.PromotionUsed.Value,
                    TotalAmountOfOrder = @event.Meter + @event.Tax
                };
                var envelope = (Envelope<ICommand>) redeemPromotion;

                _commandBus.Send(envelope);
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

            _ibs.SendPaymentNotification((double)amount, (double)meter, (double)tip, authorizationCode, orderStatusDetail.VehicleNumber, orderStatusDetail.CompanyKey);
        }

        public void Handle(OrderCancelled @event)
        {
            var orderDetail = _orderDao.FindOrderStatusById(@event.SourceId);
            if (orderDetail.IsPrepaid)
            {
                var response = _paymentService.RefundPayment(@event.SourceId);

                if (response.IsSuccessful)
                {
                    _commandBus.Send(new UpdateRefundedOrder
                    {
                        OrderId = @event.SourceId,
                        IsSuccessful = response.IsSuccessful,
                        Message = response.Message
                    });
                }
            }
            else
            {
                // void the preauthorization to prevent misuse fees
                _paymentService.VoidPreAuthorization(@event.SourceId);
            }
        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
            if (orderStatus.IsPrepaid)
            {
                _paymentService.RefundPayment(@event.SourceId);
            }
            else
            {
                // void the preauthorization to prevent misuse fees
                _paymentService.VoidPreAuthorization(@event.SourceId);
            }
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            var orderDetail = _orderDao.FindOrderStatusById(@event.SourceId);
            if (orderDetail.IsPrepaid)
            {
                var response = _paymentService.RefundPayment(@event.SourceId);

                if (response.IsSuccessful)
                {
                    _commandBus.Send(new UpdateRefundedOrder
                    {
                        OrderId = @event.SourceId,
                        IsSuccessful = response.IsSuccessful,
                        Message = response.Message
                    });
                }
            }
            else
            {
                // void the preauthorization to prevent misuse fees
                _paymentService.VoidPreAuthorization(@event.SourceId);
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                var paymentSettings = _serverSettings.GetPaymentSettings();
                var order = _orderDao.FindById(@event.SourceId);
                var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
                var pairingInfo = _orderDao.FindOrderPairingById(@event.SourceId);

                // If the user has decided not to pair (paying the ride in car instead),
                // we have to void the amount that was preauthorized
                if (paymentSettings.PaymentMode != PaymentMethod.RideLinqCmt
                    && (order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                        || order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
                    && pairingInfo == null
                    && !orderStatus.IsPrepaid) //prepaid order will never have a pairing info
                {
                    // void the preauthorization to prevent misuse fees
                    _paymentService.VoidPreAuthorization(@event.SourceId);
                }
            }
        }
    }
}