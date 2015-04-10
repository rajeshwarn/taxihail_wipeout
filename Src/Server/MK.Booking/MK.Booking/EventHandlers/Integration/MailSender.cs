﻿#region

using System;
using System.ComponentModel;
using System.Linq;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using CMTPayment;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class MailSender : IIntegrationEventHandler,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<CreditCardDeactivated>,
        IEventHandler<UserAddedToPromotionWhiteList_V2>,
        IEventHandler<ManualRideLinqTripInfoUpdated>
    {
        private readonly ICommandBus _commandBus;
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;
        private readonly IPromotionDao _promotionDao;
        private readonly IAccountDao _accountDao;
        private readonly INotificationService _notificationService;

        private readonly CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;

        public MailSender(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            ICreditCardDao creditCardDao,
            IPromotionDao promotionDao,
            IOrderDao orderDao,
            IAccountDao accountDao,
            INotificationService notificationService,
            IServerSettings serverSettings,
            ILogger logger)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _creditCardDao = creditCardDao;
            _orderDao = orderDao;
            _promotionDao = promotionDao;
            _accountDao = accountDao;
            _notificationService = notificationService;

            var cmtMobileServiceClient = new CmtMobileServiceClient(serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, logger);
        }

        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                if (@event.Status.IsPrepaid)
                {
                    // Send receipt for PrePaid
                    SendReceipt(@event.SourceId, Convert.ToDecimal(@event.Fare ?? 0), Convert.ToDecimal(@event.Tip ?? 0), Convert.ToDecimal(@event.Tax ?? 0));
                }
                else
                {
                    var order = _orderDao.FindById(@event.SourceId);
                    var pairingInfo = _orderDao.FindOrderPairingById(@event.SourceId);

                    if (order.Settings.ChargeTypeId == ChargeTypes.PaymentInCar.Id)
                    {
                        // Send receipt for Pay in Car
                        SendReceipt(@event.SourceId, Convert.ToDecimal(@event.Fare), Convert.ToDecimal(@event.Tip), Convert.ToDecimal(@event.Tax));
                    }
                    else if (pairingInfo != null && pairingInfo.DriverId.HasValue() && pairingInfo.Medallion.HasValue() && pairingInfo.PairingToken.HasValue())
                    {
                        // Send receipt for CMTRideLinq
                        var tripInfo = _cmtTripInfoServiceHelper.GetTripInfo(pairingInfo.PairingToken);
                        if (tripInfo != null && tripInfo.EndTime.HasValue)
                        {
                            var meterAmount = Math.Round(((double)tripInfo.Fare / 100), 2);
                            var tollAmount = Math.Round(((double)tripInfo.Extra / 2), 2);
                            var tipAmount = Math.Round(((double)tripInfo.Tip / 100), 2);
                            var taxAmount = Math.Round(((double)tripInfo.Tax / 100), 2);

                            SendReceipt(@event.SourceId, Convert.ToDecimal(meterAmount), Convert.ToDecimal(tipAmount), Convert.ToDecimal(taxAmount), toll: Convert.ToDecimal(tollAmount));
                        }
                    }
                } 
            }
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            if (@event.IsNoShowFee
                || @event.IsForPrepaidOrder)
            {
                // Don't message user
                // In the case of Prepaid order, he will be notified at the end of the ride
                return;
            }

            SendReceipt(@event.OrderId, @event.Meter, @event.Tip, @event.Tax, @event.AmountSavedByPromotion);
        }

        public void Handle(ManualRideLinqTripInfoUpdated @event)
        {
            var order = _orderDao.FindById(@event.SourceId);
            if (order != null)
            {
                if (@event.EndTime.HasValue)
                {
                    // ManualRideLinq order is done, send receipt

                    order.Fare = @event.Fare;
                    order.Tip = @event.Tip;
                    order.Tax = @event.Tax;
                    order.Toll = @event.Toll;
   
                    SendReceipt(@event.SourceId, Convert.ToDecimal(@event.Fare), Convert.ToDecimal(@event.Tip), Convert.ToDecimal(@event.Tax), toll: Convert.ToDecimal(@event.Toll));
                }
            }
        }

        public void Handle(CreditCardDeactivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                var creditCard = _creditCardDao.FindByAccountId(@event.SourceId).First();

                _notificationService.SendCreditCardDeactivatedEmail(creditCard.CreditCardCompany, creditCard.Last4Digits, account.Email, account.Language);
            }
        }

        public void Handle(UserAddedToPromotionWhiteList_V2 @event)
        {
            var promotion = _promotionDao.FindById(@event.SourceId);
            if (promotion == null)
            {
                return;
            }

            foreach (var accountId in @event.AccountIds)
            {
                var account = _accountDao.FindById(accountId);
                if (account != null)
                {
                    var accountLanguage = account.Language ?? SupportedLanguages.en.ToString();
                    _notificationService.SendPromotionUnlockedEmail(promotion.Name, promotion.Code, promotion.GetEndDateTime(), account.Email, accountLanguage);
                }
            }
        }

        private void SendReceipt(Guid orderId, decimal meter, decimal tip, decimal tax, decimal amountSavedByPromotion = 0m, decimal toll = 0)
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
                    
                    // Payment was handled by app, send receipt
                    if (orderStatus.IsPrepaid && orderPayment != null)
                    {
                        // Order was prepaid, all the good amounts are in OrderPaymentDetail
                        meter = orderPayment.Meter;
                        tip = orderPayment.Tip;
                        tax = orderPayment.Tax;
                    }
                        
                    var promoUsed = _promotionDao.FindByOrderId(orderId);
                    var ibsOrderId = order.IBSOrderId;

                    if (order.IsManualRideLinq)
                    {
                        var manualRideLinqDetail = context.Find<OrderManualRideLinqDetail>(orderStatus.OrderId);
                        ibsOrderId = manualRideLinqDetail.TripId;
                    }

                    var command = SendReceiptCommandBuilder.GetSendReceiptCommand(
                        order,
                        account,
                        ibsOrderId,
                        orderStatus.VehicleNumber,
                        orderStatus.DriverInfos,
                        orderPayment.SelectOrDefault(payment => Convert.ToDouble(payment.Meter), Convert.ToDouble(meter)),
                        Convert.ToDouble(toll),
                        orderPayment.SelectOrDefault(payment => Convert.ToDouble(payment.Tip), Convert.ToDouble(tip)),
                        Convert.ToDouble(tax),
                        orderPayment,
                        Convert.ToDouble(amountSavedByPromotion),
                        promoUsed,
                        card);

                    _commandBus.Send(command);
                }
            }
        }
    }
}