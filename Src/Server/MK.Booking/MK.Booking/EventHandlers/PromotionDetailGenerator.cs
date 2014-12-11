using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;
using ServiceStack.Text;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PromotionDetailGenerator : 
        IEventHandler<PromotionCreated>,
        IEventHandler<PromotionUpdated>,
        IEventHandler<PromotionActivated>,
        IEventHandler<PromotionDeactivated>,
        IEventHandler<PromotionApplied>,
        IEventHandler<PromotionRedeemed>
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
                    DaysOfWeek = @event.DaysOfWeek.ToJson(),
                    AppliesToCurrentBooking = @event.AppliesToCurrentBooking,
                    AppliesToFutureBooking = @event.AppliesToFutureBooking,
                    DiscountValue = @event.DiscountValue,
                    DiscountType = @event.DiscountType,
                    MaxUsagePerUser = @event.MaxUsagePerUser,
                    MaxUsage = @event.MaxUsage,
                    Code = @event.Code,
                    PublishedStartDate = @event.PublishedStartDate,
                    PublishedEndDate = @event.PublishedEndDate
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
                promotionDetail.DaysOfWeek = @event.DaysOfWeek.ToJson();
                promotionDetail.AppliesToCurrentBooking = @event.AppliesToCurrentBooking;
                promotionDetail.AppliesToFutureBooking = @event.AppliesToFutureBooking;
                promotionDetail.DiscountValue = @event.DiscountValue;
                promotionDetail.DiscountType = @event.DiscountType;
                promotionDetail.MaxUsagePerUser = @event.MaxUsagePerUser;
                promotionDetail.MaxUsage = @event.MaxUsage;
                promotionDetail.Code = @event.Code;
                promotionDetail.PublishedStartDate = @event.PublishedStartDate;
                promotionDetail.PublishedEndDate = @event.PublishedEndDate;

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

        public void Handle(PromotionApplied @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new PromotionUsageDetail
                {
                    OrderId = @event.OrderId,
                    PromoId = @event.SourceId,
                    @AccountId = @event.AccountId,
                    Code = @event.Code,
                    DiscountType = @event.DiscountType,
                    DiscountValue = @event.DiscountValue
                });
            }
        }

        public void Handle(PromotionRedeemed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promotionUsageDetail = context.Find<PromotionUsageDetail>(@event.OrderId);

                promotionUsageDetail.AmountSaved = @event.AmountSaved;

                // Update usage statistics
                var promotion = context.Find<PromotionDetail>(@event.PromoId);
                var promotionStatistics = context.Find<PromotionStatisticDetail>(@event.PromoId);
                var account = context.Find<AccountDetail>(promotionUsageDetail.AccountId);

                if (promotionStatistics == null)
                {
                    // First usage of promotion
                    context.Save(new PromotionStatisticDetail
                    {
                        Id = @event.PromoId,
                        PromoCode = promotion.Code,
                        TotalUsageAmount = (double)@event.AmountSaved,
                        UsageCount = 1,
                        UsersUsage = new Dictionary<string, int>{ {account.Email, 1} }
                    });
                }
                else
                {
                    promotionStatistics.TotalUsageAmount += (double)@event.AmountSaved;
                    promotionStatistics.UsageCount++;

                    // Add user usage statistic
                    if (promotionStatistics.UsersUsage.ContainsKey(account.Email))
                    {
                        promotionStatistics.UsersUsage[account.Email]++;
                    }
                    else
                    {
                        promotionStatistics.UsersUsage.Add(account.Email, 1);
                    }
                }

                context.SaveChanges();
            }
        }
    }
}