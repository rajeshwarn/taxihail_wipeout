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
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<PromotionCreated>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderCancelledBecauseOfError>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly IPromotionDao _promotionDao;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;

        public PromotionTriggerGenerator(Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            IPromotionDao promotionDao,
            IAccountDao accountDao,
            IOrderDao orderDao)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _promotionDao = promotionDao;
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        public void Handle(AccountRegistered @event)
        {
            // Check if exists promotion triggered on account creation
            var accountCreatedPromotion = _promotionDao.GetAllCurrentlyActive(PromotionTriggerTypes.AccountCreated).FirstOrDefault();

            if (accountCreatedPromotion != null)
            {
                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountIds = new[] { @event.SourceId },
                    PromoId = accountCreatedPromotion.Id
                });
            }
        }

        public void Handle(PromotionCreated @event)
        {
            if (@event.TriggerSettings.Type == PromotionTriggerTypes.AccountCreated
                && @event.TriggerSettings.ApplyToExisting)
            {
                // Get all accounts
                var existingAccounts = _accountDao.GetAll();

                // Get all account ids
                var accoundIds = existingAccounts.Select(accountDetail => accountDetail.Id).ToArray();

                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountIds = accoundIds,
                    PromoId = @event.SourceId
                });
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

            var activePromotions = _promotionDao.GetAllCurrentlyActive().ToArray();

            UpdateRideProgression(activePromotions, PromotionTriggerTypes.AmountSpent, @event.AccountId, @event.OrderId, (double)@event.Meter);
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
                    var eligibleOrdersQuery =
                        context.Set<OrderDetail>()
                            .Where(
                                x => x.Status == (int)OrderStatus.Completed
                                     && x.AccountId == accountId
                                     && x.PickupDate >= promoStartDate);

                    // Check for promotion end date
                    if (promoEndDate.HasValue)
                    {
                        eligibleOrdersQuery = eligibleOrdersQuery.Where(x => x.PickupDate < promoEndDate);
                    }

                    var eligibleOrders = eligibleOrdersQuery.ToArray();

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
                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountIds = new[] { accountId },
                    PromoId = promotion.Id,
                    LastTriggeredAmount = promotionProgress
                });
            }
        }

        public void Handle(OrderCancelled @event)
        {
            SendUnapplyPromotionCommand(@event.SourceId);
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            SendUnapplyPromotionCommand(@event.SourceId);
        }

        private void SendUnapplyPromotionCommand(Guid orderId)
        {
            var orderDetail = _orderDao.FindById(orderId);
            var promotionDetail = _promotionDao.FindByOrderId(orderId);

            // There was no promotion applied to this order.
            if (promotionDetail == null)
            {
                return;
            }

                _commandBus.Send(new UnapplyPromotion
                {
                    PromoId = promotionDetail.PromoId,
                    AccountId = orderDetail.AccountId,
                    OrderId = orderId
                });
            }
        }
    }
}
