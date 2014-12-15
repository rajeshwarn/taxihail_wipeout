using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;

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
                return context.Query<PromotionDetail>().OrderBy(x => x.Active).ThenBy(x => x.Name).ToList();
            }
        }

        public IEnumerable<PromotionProgressDetail> GetAllProgress()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionProgressDetail>().ToArray();
            }
        }

        public IEnumerable<PromotionDetail> GetAllCurrentlyActive()
        {
            using (var context = _contextFactory.Invoke())
            {
                var result = new List<PromotionDetail>();
                var activePromos = context.Query<PromotionDetail>().Where(x => x.Active);
                foreach (var promotionDetail in activePromos)
                {
                    var thisPromo = promotionDetail;
                    if (thisPromo.PublishedStartDate.HasValue || thisPromo.PublishedEndDate.HasValue)
                    {
                        // at least one published date set, so it's public
                        
                        if (!thisPromo.PublishedStartDate.HasValue)
                        {
                            thisPromo.PublishedStartDate = DateTime.MinValue;
                        }

                        if (!thisPromo.PublishedEndDate.HasValue)
                        {
                            thisPromo.PublishedEndDate = DateTime.MaxValue;
                        }

                        var now = GetCurrentOffsetedTime();
                        if (thisPromo.PublishedStartDate <= now
                            && thisPromo.PublishedEndDate > now
                            && !IsExpired(thisPromo, now))
                        {
                            result.Add(promotionDetail);
                        }
                    }
                }

                return result;
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