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
    }
}