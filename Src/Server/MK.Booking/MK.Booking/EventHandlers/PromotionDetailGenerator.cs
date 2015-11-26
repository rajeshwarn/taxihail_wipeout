using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;
using ServiceStack.Text;
using apcurium.MK.Booking.Projections;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PromotionDetailGenerator : 
        IEventHandler<PromotionCreated>,
        IEventHandler<PromotionUpdated>,
        IEventHandler<PromotionActivated>,
        IEventHandler<PromotionDeactivated>,
        IEventHandler<PromotionApplied>,
        IEventHandler<PromotionUnapplied>,
        IEventHandler<PromotionRedeemed>,
        IEventHandler<UserAddedToPromotionWhiteList_V2>,
        IEventHandler<PromotionDeleted>
    {
        private readonly IProjectionSet<AccountDetail> _accountDetailProjectionSet;
        private readonly IProjectionSet<PromotionDetail> _promotionProjectionSet;
        private readonly IProjectionSet<PromotionUsageDetail> _promotionUsageProjectionSet;
        private readonly PromotionProgressDetailProjectionSet _promotionProgressProjectionSet;

        public PromotionDetailGenerator(IProjectionSet<AccountDetail> accountDetailProjectionSet,
            IProjectionSet<PromotionDetail> promotionProjectionSet,
            IProjectionSet<PromotionUsageDetail> promotionUsageProjectionSet,
            PromotionProgressDetailProjectionSet promotionProgressProjectionSet)
        {
            _accountDetailProjectionSet = accountDetailProjectionSet;
            _promotionProjectionSet = promotionProjectionSet;
            _promotionUsageProjectionSet = promotionUsageProjectionSet;
            _promotionProgressProjectionSet = promotionProgressProjectionSet;
        }

        public void Handle(PromotionCreated @event)
        {
            _promotionProjectionSet.Add(new PromotionDetail
            {
                Id = @event.SourceId,
                Name = @event.Name,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                StartTime = @event.StartTime,
                EndTime = @event.EndTime,
                DaysOfWeek = @event.DaysOfWeek.ToJson(),
                AppliesToCurrentBooking = @event.AppliesToCurrentBooking,
                AppliesToFutureBooking = @event.AppliesToFutureBooking,
                DiscountValue = @event.DiscountValue,
                DiscountType = @event.DiscountType,
                MaxUsagePerUser = @event.MaxUsagePerUser,
                MaxUsage = @event.MaxUsage,
                Code = @event.Code,
                PublishedStartDate = @event.PublishedStartDate,
                PublishedEndDate = @event.PublishedEndDate,
                TriggerSettings = @event.TriggerSettings,
                Active = true,
                Deleted = false
            });
        }

        public void Handle(PromotionUpdated @event)
        {
            _promotionProjectionSet.Update(@event.SourceId, promotion =>
            {
                promotion.Name = @event.Name;
                promotion.Description = @event.Description;
                promotion.StartDate = @event.StartDate;
                promotion.EndDate = @event.EndDate;
                promotion.StartTime = @event.StartTime;
                promotion.EndTime = @event.EndTime;
                promotion.DaysOfWeek = @event.DaysOfWeek.ToJson();
                promotion.AppliesToCurrentBooking = @event.AppliesToCurrentBooking;
                promotion.AppliesToFutureBooking = @event.AppliesToFutureBooking;
                promotion.DiscountValue = @event.DiscountValue;
                promotion.DiscountType = @event.DiscountType;
                promotion.MaxUsagePerUser = @event.MaxUsagePerUser;
                promotion.MaxUsage = @event.MaxUsage;
                promotion.Code = @event.Code;
                promotion.PublishedStartDate = @event.PublishedStartDate;
                promotion.PublishedEndDate = @event.PublishedEndDate;
                promotion.TriggerSettings = @event.TriggerSettings;
            });
        }

        public void Handle(PromotionActivated @event)
        {
            _promotionProjectionSet.Update(@event.SourceId, promotion =>
            {
                promotion.Active = true;
            });
        }

        public void Handle(PromotionDeactivated @event)
        {
            _promotionProjectionSet.Update(@event.SourceId, promotion =>
            {
                promotion.Active = false;
            });
        }

        public void Handle(PromotionDeleted @event)
        {
            _promotionProjectionSet.Update(@event.SourceId, promotion =>
            {
                promotion.Deleted = true;
            });
        }

        public void Handle(PromotionApplied @event)
        {
            var account = _accountDetailProjectionSet.GetProjection(@event.AccountId).Load();

            _promotionUsageProjectionSet.Add(new PromotionUsageDetail
            {
                OrderId = @event.OrderId,
                PromoId = @event.SourceId,
                AccountId = @event.AccountId,
                UserEmail = account.Email,
                Code = @event.Code,
                DiscountType = @event.DiscountType,
                DiscountValue = @event.DiscountValue
            });
        }

        public void Handle(PromotionUnapplied @event)
        {
            _promotionUsageProjectionSet.Remove(@event.OrderId);
        }

        public void Handle(PromotionRedeemed @event)
        {
            _promotionUsageProjectionSet.Update(@event.OrderId, promoUsage =>
            {
                promoUsage.AmountSaved = @event.AmountSaved;
                promoUsage.DateRedeemed = @event.EventDate;
            });
        }

        public void Handle(UserAddedToPromotionWhiteList_V2 @event)
        {
            foreach (var accountId in @event.AccountIds)
            {
                _promotionProgressProjectionSet.Update(accountId, list =>
                {
                    var existing = list.FirstOrDefault(x => x.AccountId == accountId && x.PromoId == @event.SourceId);

                    if (existing == null)
                    {
                        list.Add(new PromotionProgressDetail
                        {
                            AccountId = accountId,
                            PromoId = @event.SourceId,
                            LastTriggeredAmount = @event.LastTriggeredAmount
                        });
                    }
                    else
                    {
                        existing.LastTriggeredAmount = @event.LastTriggeredAmount;
                    }
                });
            }
        }
    }
}