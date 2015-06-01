﻿using System;
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
        private readonly IFeeService _feeService;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;

        public OrderPaymentManager(IOrderDao dao, IOrderPaymentDao paymentDao, IAccountDao accountDao, IOrderDao orderDao, ICommandBus commandBus,
            ICreditCardDao creditCardDao, IIbsOrderService ibs, IServerSettings serverSettings, IPaymentService paymentService, IFeeService feeService)
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
            _feeService = feeService;
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            // Migration
            @event.MigrateFees();

            if (@event.FeeType != FeeTypes.None)
            {
                // Don't message driver
                return;
            }

            var taxedMeterAmount = @event.Meter + @event.Tax;

            if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver
                && !@event.IsSettlingOverduePayment) // Don't send notification to driver when user settles overdue payment
            {
                // To prevent driver confusion we will not send the discounted total amount for the fare.
                // We will also not send booking fee since it could be from a market company and the driver would not know where it's coming from.
                var totalAmountBeforePromotionAndBookingFees = @event.Amount + @event.AmountSavedByPromotion - @event.BookingFees;
                SendPaymentConfirmationToDriver(@event.OrderId, totalAmountBeforePromotionAndBookingFees, taxedMeterAmount, @event.Tip, @event.Provider.ToString(), @event.AuthorizationCode);
            }

            if (@event.PromotionUsed.HasValue)
            {
                var redeemPromotion = new RedeemPromotion
                {
                    OrderId = @event.OrderId,
                    PromoId = @event.PromotionUsed.Value,
                    TaxedMeterAmount = taxedMeterAmount // MK: Booking fees don't count towards promo rebate (2015/05/25)
                };

                var envelope = (Envelope<ICommand>)redeemPromotion;

                _commandBus.Send(envelope);
            }
        }

        private void SendPaymentConfirmationToDriver(Guid orderId, decimal totalAmountBeforePromotion, decimal taxedMeterAmount, decimal tipAmount, string provider,  string authorizationCode)
        {
            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(orderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(orderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");
            if (orderDetail.IBSOrderId == null) throw new InvalidOperationException("IBSOrderId should not be null");

            var payment = _paymentDao.FindByOrderId(orderId, orderDetail.CompanyKey);
            if (payment == null) throw new InvalidOperationException("Payment info not found");

            var account = _accountDao.FindById(orderDetail.AccountId);
            if (account == null) throw new InvalidOperationException("Order account not found");

            if ( provider == PaymentType.CreditCard.ToString () )
            {
                var card = _creditCardDao.FindByToken(payment.CardToken);
                if (card == null) throw new InvalidOperationException("Credit card not found");
            }

            _ibs.SendPaymentNotification((double)totalAmountBeforePromotion, (double)taxedMeterAmount, (double)tipAmount, authorizationCode, orderStatusDetail.VehicleNumber);
        }

        public void Handle(OrderCancelled @event)
        {
            var orderDetail = _orderDao.FindOrderStatusById(@event.SourceId);
            if (orderDetail.IsPrepaid)
            {
                var response = _paymentService.RefundPayment(orderDetail.CompanyKey, @event.SourceId);

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
                var feeCharged = _feeService.ChargeCancellationFeeIfNecessary(orderDetail);

                if (orderDetail.CompanyKey != null)
                {
                    // Company not-null will never (so far) perceive no show fees, so we need to void its preauth
                    _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, orderDetail.OrderId);
                }
                else
                {
                    if (!feeCharged.HasValue)
                    {
                        // No fees were charged on company null, void the preauthorization to prevent misuse fees
                        _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, @event.SourceId);
                    }
                }
            }
        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            if (@event.HasChangedBackToPaymentInCar)
            {
                var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
                if (orderStatus.IsPrepaid)
                {
                    _paymentService.RefundPayment(orderStatus.CompanyKey, @event.SourceId);
                }
                else
                {
                    // void the preauthorization to prevent misuse fees
                    _paymentService.VoidPreAuthorization(orderStatus.CompanyKey, @event.SourceId);
                }
            }
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            var orderDetail = _orderDao.FindOrderStatusById(@event.SourceId);
            if (orderDetail.IsPrepaid)
            {
                var response = _paymentService.RefundPayment(orderDetail.CompanyKey, @event.SourceId);

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
                _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, @event.SourceId);
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                var order = _orderDao.FindById(@event.SourceId);
                var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
                var pairingInfo = _orderDao.FindOrderPairingById(@event.SourceId);

                if (_serverSettings.GetPaymentSettings(order.CompanyKey).PaymentMode == PaymentMethod.RideLinqCmt)
                {
                    // Since RideLinqCmt payment is processed automatically by CMT, we have to charge booking fees separately
                    _feeService.ChargeBookingFeesIfNecessary(orderStatus);
                }

                // If the user has decided not to pair (paying the ride in car instead),
                // we have to void the amount that was preauthorized
                if (_serverSettings.GetPaymentSettings(order.CompanyKey).PaymentMode != PaymentMethod.RideLinqCmt
                    && (order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id || order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
                    && pairingInfo == null
                    && !orderStatus.IsPrepaid) //prepaid order will never have a pairing info
                {
                    // void the preauthorization to prevent misuse fees
                    _paymentService.VoidPreAuthorization(order.CompanyKey, @event.SourceId);
                }
            }
        }
    }
}