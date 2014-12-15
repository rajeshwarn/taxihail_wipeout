using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PromotionTriggerGenerator :
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<AccountConfirmed>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly IPromotionDao _promotionDao;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly INotificationService _notificationService;

        public PromotionTriggerGenerator(
            Func<BookingDbContext> contextFactory,
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

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            var accountId = _orderDao.FindById(@event.OrderId).AccountId;
            var activePromotions = _promotionDao.GetAllCurrentlyActive().ToArray();

            // Update ride count promotions progression
            UpdateProgression(activePromotions, PromotionTriggerTypes.RideCount, accountId);

            // Update amount spent promotions progression
            UpdateProgression(activePromotions, PromotionTriggerTypes.AmountSpent, accountId, (double)@event.Amount);
        }

        public void Handle(AccountConfirmed @event)
        {
            // Check if exists promotion triggered on account creation
            var activePromotions = _promotionDao.GetAllCurrentlyActive();
            var accountCreatedPromotion =
                activePromotions.FirstOrDefault(
                    p => p.TriggerSettings.Type == PromotionTriggerTypes.AccountCreated.Id);

            if (accountCreatedPromotion != null)
            {
                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountId = @event.SourceId,
                    PromoId = accountCreatedPromotion.Id
                });
            }
        }

        private void UpdateProgression(IEnumerable<PromotionDetail> activePromotions, ListItem triggerType, Guid accountId, double? value = null)
        {
            if (value == null
                && triggerType.Id == PromotionTriggerTypes.AmountSpent.Id)
            {
                throw new ArgumentNullException("value", "Value parameter should not be null with Amount Spent triggerType");
            }

            using (var context = _contextFactory.Invoke())
            {
                var promotions = activePromotions.Where(p => p.TriggerSettings.Type == triggerType.Id);

                foreach (var promotion in promotions)
                {
                    var promotionProgress = context.Set<PromotionProgressDetail>().Find(accountId, promotion.Id);
                    if (promotionProgress == null)
                    {
                        promotionProgress = new PromotionProgressDetail { AccountId = accountId, PromoId = promotion.Id };
                        context.Save(promotionProgress);
                    }

                    bool promotionUnlocked;

                    if (triggerType.Id == PromotionTriggerTypes.RideCount.Id)
                    {
                        promotionProgress.RideCount = promotionProgress.RideCount.HasValue
                            ? promotionProgress.RideCount + 1
                            : 1;

                        promotionUnlocked = UnlockPromotionIfNecessary(promotion.TriggerSettings.RideCount, promotionProgress.RideCount.Value, accountId, promotion);
                        if (promotionUnlocked)
                        {
                            // Reset promotion progress
                            promotionProgress.RideCount = 0;
                        }
                    }
                    else if (triggerType.Id == PromotionTriggerTypes.AmountSpent.Id)
                    {
                        promotionProgress.AmountSpent = promotionProgress.AmountSpent.HasValue
                            ? promotionProgress.AmountSpent + value
                            : value;

                        promotionUnlocked = UnlockPromotionIfNecessary(promotion.TriggerSettings.AmountSpent, promotionProgress.AmountSpent.Value, accountId, promotion);
                        if (promotionUnlocked)
                        {
                            // Reset promotion progress
                            promotionProgress.AmountSpent = 0.0;
                        }
                    }
                }

                context.SaveChanges();
            }
        }

        private bool UnlockPromotionIfNecessary(double promotionThreshold, double promotionProgress, Guid accountId, PromotionDetail promotion)
        {
            if (promotionProgress >= promotionThreshold)
            {
                _commandBus.Send(new AddUserToPromotionWhiteList
                {
                    AccountId = accountId,
                    PromoId = promotion.Id
                });

                var account = _accountDao.FindById(accountId);

                _notificationService.SendPromotionUnlockedEmail(promotion.Name, promotion.Code, promotion.GetEndDateTime(), account.Email, account.Language);

                return true;
            }
            return false;
        }
    }
}
