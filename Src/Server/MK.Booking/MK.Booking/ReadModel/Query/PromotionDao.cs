using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class PromotionDao : IPromotionDao
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IClock _clock;
        private readonly IServerSettings _serverSettings;

        public PromotionDao(Func<BookingDbContext> contextFactory, IClock clock, IServerSettings serverSettings)
        {
            _contextFactory = contextFactory;
            _clock = clock;
            _serverSettings = serverSettings;
        }

        public IEnumerable<PromotionDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionDetail>().OrderBy(x => x.Active).ThenBy(x => x.Name).ToArray();
            }
        }

        public IEnumerable<PromotionProgressDetail> GetProgressByPromo(Guid promoId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionProgressDetail>().Where(x => x.PromoId == promoId).ToArray();
            }
        }

        public PromotionProgressDetail GetProgress(Guid accountId, Guid promoId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<PromotionProgressDetail>().Find(accountId, promoId);
            }
        }

        public IEnumerable<PromotionDetail> GetAllCurrentlyActiveAndPublished(PromotionTriggerTypes? triggerType = null)
        {
            return GetAllCurrentlyActive(triggerType)
                // at least one published date set, so it's public
                .Where(promotionDetail => promotionDetail.PublishedStartDate.HasValue || promotionDetail.PublishedEndDate.HasValue)
                .Select(promotionDetail =>
                {
                    if (!promotionDetail.PublishedStartDate.HasValue)
                    {
                        promotionDetail.PublishedStartDate = DateTime.MinValue;
                    }

                    if (!promotionDetail.PublishedEndDate.HasValue)
                    {
                        promotionDetail.PublishedEndDate = DateTime.MaxValue;
                    }

                    return promotionDetail;
                })
                .Where(promotionDetail =>
                {
                    var now = GetCurrentOffsetedTime();

                    return promotionDetail.PublishedStartDate <= now
                           && promotionDetail.PublishedEndDate > now;
                });
        }

        public IEnumerable<PromotionDetail> GetAllCurrentlyActive(PromotionTriggerTypes? triggerType = null)
        {
            using (var context = _contextFactory.Invoke())
            {
                var activePromos = context.Query<PromotionDetail>().Where(x => x.Active);

                if (triggerType.HasValue)
                {
                    activePromos = activePromos.Where(x => x.TriggerSettings.Type == triggerType);
                }

                return activePromos
                    .AsEnumerable()
                    .Where(promotionDetail =>
                    {
                        var now = GetCurrentOffsetedTime();

                        return !IsExpired(promotionDetail, now);
                    })
                    .ToArray(); 
            }
        }

        public PromotionDetail FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionDetail>().SingleOrDefault(c => c.Id == id);
            }
        }

        public PromotionDetail FindByPromoCode(string promoCode)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionDetail>().SingleOrDefault(c => c.Code == promoCode);
            }
        }

        public PromotionUsageDetail FindByOrderId(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionUsageDetail>().SingleOrDefault(c => c.OrderId == orderId);
            }
        }

        public IEnumerable<PromotionUsageDetail> GetRedeemedPromotionUsages(Guid promoId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionUsageDetail>().Where(c => c.PromoId == promoId && c.DateRedeemed.HasValue).ToArray();
            }
        }

        private DateTime GetCurrentOffsetedTime()
        {
            var ibsServerTimeDifference = _serverSettings.ServerData.IBS.TimeDifference;
            var now = _clock.Now;
            if (ibsServerTimeDifference != 0)
            {
                now = now.Add(new TimeSpan(ibsServerTimeDifference));
            }

            return now;
        }


        private bool IsStarted(PromotionDetail promo, DateTime now)
        {
            var startDateTime = promo.GetStartDateTime().GetValueOrDefault(DateTime.MinValue);

            return startDateTime <= now;
        }

        private bool IsExpired(PromotionDetail promo, DateTime now)
        {
            var endDateTime = promo.GetEndDateTime();

            if (endDateTime.HasValue && endDateTime.Value <= now)
            {
                return true;
            }

            return false;
        }
    }
}