using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PromotionTriggerGenerator : IIntegrationEventHandler,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<AccountRegistered>,
        IEventHandler<CreditCardPaymentCaptured_V2>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly IPromotionDao _promotionDao;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly INotificationService _notificationService;

        public PromotionTriggerGenerator(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            IPromotionDao promotionDao,
            IOrderDao orderDao,
            IAccountDao accountDao,
            INotificationService notificationService)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _promotionDao = promotionDao;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _notificationService = notificationService;
        }

        public void Handle(AccountRegistered @event)
        {
            // Check if exists promotion triggered on account creation
            var accountCreatedPromotion = _promotionDao.GetAllCurrentlyActive(PromotionTriggerTypes.AccountCreated).FirstOrDefault();

            if (accountCreatedPromotion != null)
            {
                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountId = @event.SourceId,
                    PromoId = accountCreatedPromotion.Id
                });

                //_notificationService.SendPromotionUnlockedEmail(accountCreatedPromotion.Name,
                //    accountCreatedPromotion.Code,
                //    accountCreatedPromotion.GetEndDateTime(),
                //    @event.Email,
                //    @event.Language);
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            if (!@event.IsCompleted)
            {
                return;
            }

            var accountId = @event.Status.AccountId;
            var activePromotions = _promotionDao.GetAllCurrentlyActive().ToArray();

            UpdateRideProgression(activePromotions, PromotionTriggerTypes.RideCount, accountId, @event.SourceId);
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            if (@event.IsNoShowFee)
            {
                return;
            }

            var accountId = _orderDao.FindById(@event.OrderId).AccountId;
            var activePromotions = _promotionDao.GetAllCurrentlyActive().ToArray();

            UpdateRideProgression(activePromotions, PromotionTriggerTypes.AmountSpent, accountId, @event.OrderId, (double)@event.Meter);
        }

        private void UpdateRideProgression(IEnumerable<PromotionDetail> activePromotions, PromotionTriggerTypes triggerType, Guid accountId, Guid orderId, double? value = null)
        {
            if (value == null
                && triggerType == PromotionTriggerTypes.AmountSpent)
            {
                throw new ArgumentNullException("value", "Value parameter should not be null with Amount Spent triggerType");
            }

            using (var context = _contextFactory.Invoke())
            {
                var promotions = activePromotions.Where(p => p.TriggerSettings.Type == triggerType);

                foreach (var promotion in promotions)
                {
                    var promoStartDate = promotion.GetStartDateTime();
                    var promoEndDate = promotion.GetEndDateTime();

                    // Get all orders from account that are in the date range of the promotion
                    var eligibleOrders =
                        context.Set<OrderDetail>()
                            .Where(
                                x => x.Status == (int)OrderStatus.Completed
                                     && x.AccountId == accountId
                                     && x.PickupDate >= promoStartDate
                                     && x.PickupDate < promoEndDate).ToArray();

                    if (promotion.TriggerSettings.Type == PromotionTriggerTypes.RideCount)
                    {
                        var orderCount = eligibleOrders.Any(x => x.Id == orderId)
                            ? eligibleOrders.Length
                            : eligibleOrders.Length + 1;

                        UnlockPromotionIfNecessary(orderCount, promotion.TriggerSettings.RideCount, triggerType, accountId, promotion);
                    }
                    else if (promotion.TriggerSettings.Type == PromotionTriggerTypes.AmountSpent)
                    {
                        var promotionProgressDetail = context.Set<PromotionProgressDetail>().Find(accountId, promotion.Id);
                        double lastTriggeredAmount = 0;

                        if (promotionProgressDetail != null && promotionProgressDetail.LastTriggeredAmount.HasValue)
                        {
                            lastTriggeredAmount = promotionProgressDetail.LastTriggeredAmount.Value;
                        }

                        var totalAmountSpent = eligibleOrders.Any(x => x.Id == orderId)
                            ? eligibleOrders.Sum(x => x.Fare.GetValueOrDefault() + x.Tax.GetValueOrDefault() + x.Toll.GetValueOrDefault())
                            : eligibleOrders.Sum(x => x.Fare.GetValueOrDefault() + x.Tax.GetValueOrDefault() + x.Toll.GetValueOrDefault()) + value;

                        // To get the current progress of the promo, we need to calculate only from the last time the promo was triggered
                        var amountSpentProgress = totalAmountSpent.GetValueOrDefault() - lastTriggeredAmount;

                        UnlockPromotionIfNecessary(amountSpentProgress, promotion.TriggerSettings.AmountSpent, triggerType, accountId, promotion);
                    }
                }
            }
        }

        private void UnlockPromotionIfNecessary(double promotionProgress, double promotionThreshold, PromotionTriggerTypes triggerType, Guid accountId, PromotionDetail promotion)
        {
            // Check if promotion needs to be unlocked
            bool isPromotionUnlocked = false;

            if (triggerType == PromotionTriggerTypes.RideCount)
            {
                isPromotionUnlocked = promotionProgress != 0
                    && promotionProgress >= promotionThreshold
                    && (promotionProgress % promotionThreshold) == 0;
            }
            else if (triggerType == PromotionTriggerTypes.AmountSpent)
            {
                isPromotionUnlocked = promotionProgress >= promotionThreshold;
            }

            if (isPromotionUnlocked)
            {
                using (var context = _contextFactory.Invoke())
                {
                    var promotionProgressDetail = context.Set<PromotionProgressDetail>().Find(accountId, promotion.Id);
                    if (promotionProgressDetail == null)
                    {
                        promotionProgressDetail = new PromotionProgressDetail { AccountId = accountId, PromoId = promotion.Id };
                        context.Save(promotionProgressDetail);
                    }

                    promotionProgressDetail.LastTriggeredAmount = promotionProgress;
                    context.SaveChanges();
                }

                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountId = accountId,
                    PromoId = promotion.Id
                });

                var account = _accountDao.FindById(accountId);
                if (account != null)
                {
                    _notificationService.SendPromotionUnlockedEmail(promotion.Name, promotion.Code, promotion.GetEndDateTime(), account.Email, account.Language);
                }
            }
        }
    }
}
