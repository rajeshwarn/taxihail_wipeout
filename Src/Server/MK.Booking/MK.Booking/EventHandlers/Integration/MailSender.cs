#region

using System;
using System.Linq;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class MailSender : IIntegrationEventHandler,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<CreditCardDeactivated>,
        IEventHandler<UserAddedToPromotionWhiteList_V2>
    {
        private readonly ICommandBus _commandBus;
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;
        private readonly IPromotionDao _promotionDao;
        private readonly IAccountDao _accountDao;
        private readonly INotificationService _notificationService;

        public MailSender(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            ICreditCardDao creditCardDao,
            IPromotionDao promotionDao,
            IOrderDao orderDao,
            IAccountDao accountDao,
            INotificationService notificationService)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _creditCardDao = creditCardDao;
            _orderDao = orderDao;
            _promotionDao = promotionDao;
            _accountDao = accountDao;
            _notificationService = notificationService;
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

        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                if (@event.Status.IsPrepaid)
                {
                    SendReceipt(@event.SourceId, Convert.ToDecimal(@event.Fare ?? 0), Convert.ToDecimal(@event.Tip ?? 0), Convert.ToDecimal(@event.Tax ?? 0));
                }
                else
                {
                    var order = _orderDao.FindById(@event.SourceId);

                    if (order.Settings.ChargeTypeId == ChargeTypes.PaymentInCar.Id)
                    {
                        SendReceipt(@event.SourceId, Convert.ToDecimal(@event.Fare), Convert.ToDecimal(@event.Tip), Convert.ToDecimal(@event.Tax));
                    }
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

        private void SendReceipt(Guid orderId, decimal meter, decimal tip, decimal tax, decimal amountSavedByPromotion = 0m)
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
                    
                    // payment was handled by app, send receipt
                    if (orderPayment != null && orderStatus.IsPrepaid)
                    {
                        // Order was prepaid, all the good amounts are in OrderPaymentDetail
                        meter = orderPayment.Meter;
                        tip = orderPayment.Tip;
                        tax = orderPayment.Tax;
                    }
                        
                    var promoUsed = _promotionDao.FindByOrderId(orderId);

                    var command = SendReceiptCommandBuilder.GetSendReceiptCommand(
                        order,
                        account,
                        orderStatus.VehicleNumber,
                        orderStatus.DriverInfos,
                        orderPayment.SelectOrDefault(payment => Convert.ToDouble(payment.Meter), Convert.ToDouble(meter)),
                        0,
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