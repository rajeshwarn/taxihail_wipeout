using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

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
        private readonly Func<BookingDbContext> _contextFactory;

        public PromotionDetailGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(PromotionCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionDetail = new PromotionDetail
                {
                    Id = @event.SourceId,
                    Name = @event.Name,
                    Description = @event.Description,
                    StartDate = @event.StartDate,
                    EndDate = @event.EndDate,
                    StartTime = @event.StartTime,
                    EndTime = @event.EndTime,
                    DaysOfWeek = @event.DaysOfWeek.Select(daysOfWeek => daysOfWeek.ToString()).ToArray().ToJson(),
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
                };

                context.Save(promotionDetail);
            }
        }

        public void Handle(PromotionUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionDetail = context.Find<PromotionDetail>(@event.SourceId);

                promotionDetail.Name = @event.Name;
                promotionDetail.Description = @event.Description;
                promotionDetail.StartDate = @event.StartDate;
                promotionDetail.EndDate = @event.EndDate;
                promotionDetail.StartTime = @event.StartTime;
                promotionDetail.EndTime = @event.EndTime;
                promotionDetail.DaysOfWeek = @event.DaysOfWeek.Select(daysOfWeek => daysOfWeek.ToString()).ToArray().ToJson();
                promotionDetail.AppliesToCurrentBooking = @event.AppliesToCurrentBooking;
                promotionDetail.AppliesToFutureBooking = @event.AppliesToFutureBooking;
                promotionDetail.DiscountValue = @event.DiscountValue;
                promotionDetail.DiscountType = @event.DiscountType;
                promotionDetail.MaxUsagePerUser = @event.MaxUsagePerUser;
                promotionDetail.MaxUsage = @event.MaxUsage;
                promotionDetail.Code = @event.Code;
                promotionDetail.PublishedStartDate = @event.PublishedStartDate;
                promotionDetail.PublishedEndDate = @event.PublishedEndDate;
                promotionDetail.TriggerSettings = @event.TriggerSettings;

                context.Save(promotionDetail);
            }
        }

        public void Handle(PromotionActivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionDetail = context.Find<PromotionDetail>(@event.SourceId);

                promotionDetail.Active = true;

                context.Save(promotionDetail);
            }
        }

        public void Handle(PromotionDeactivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionDetail = context.Find<PromotionDetail>(@event.SourceId);

                promotionDetail.Active = false;

                context.Save(promotionDetail);
            }
        }

        public void Handle(PromotionDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionDetail = context.Find<PromotionDetail>(@event.SourceId);

                promotionDetail.Deleted = true;

                context.SaveChanges();
            }
        }

        public void Handle(PromotionApplied @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.AccountId);

                context.Save(new PromotionUsageDetail
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
        }

        public void Handle(PromotionUnapplied @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionUsageDetail = context.Find<PromotionUsageDetail>(@event.OrderId);
                if (promotionUsageDetail != null)
                {
                    context.Set<PromotionUsageDetail>().Remove(promotionUsageDetail);
                    context.Save(promotionUsageDetail);
                }
            }
        }

        public void Handle(PromotionRedeemed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionUsageDetail = context.Find<PromotionUsageDetail>(@event.OrderId);                
                
                promotionUsageDetail.AmountSaved = @event.AmountSaved;
                promotionUsageDetail.DateRedeemed = @event.EventDate;

                context.SaveChanges();
            }
        }

        public void Handle(UserAddedToPromotionWhiteList_V2 @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                foreach (var accountId in @event.AccountIds)
                {
                    var promotionProgressDetail = context.Set<PromotionProgressDetail>().Find(accountId, @event.SourceId);
                    if (promotionProgressDetail == null)
                    {
                        promotionProgressDetail = new PromotionProgressDetail { AccountId = accountId, PromoId = @event.SourceId };
                        context.Save(promotionProgressDetail);
                    }

                    promotionProgressDetail.LastTriggeredAmount = @event.LastTriggeredAmount;
                }
                context.SaveChanges();
            }
        }
    }
}