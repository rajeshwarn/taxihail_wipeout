using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class PromotionDao : IPromotionDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PromotionDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IEnumerable<PromotionDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PromotionDetail>().OrderBy(x => x.Active).ThenBy(x => x.Name).ToList();
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

                        //TODO PROMO: we should abstract Now with a IClock http://stackoverflow.com/questions/43711/whats-a-good-way-to-overwrite-datetime-now-during-testing
                        //TODO PROMO: should we be using an UTC, date need to be on the same timezone, admin in France comparing a server in the US ?
                        var now = DateTime.Now.Date;
                        if (thisPromo.PublishedStartDate <= now
                            && thisPromo.PublishedEndDate > now)
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
    }
}