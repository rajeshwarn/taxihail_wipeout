using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class RatingTypeDao : IRatingTypeDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public RatingTypeDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<RatingTypeDetail> GetAll()
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<RatingTypeDetail>().ToList();
            }
        }

        public RatingTypeDetail GetById(Guid id)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<RatingTypeDetail>().SingleOrDefault(r => r.Id == id);
            }
        }
    }
}