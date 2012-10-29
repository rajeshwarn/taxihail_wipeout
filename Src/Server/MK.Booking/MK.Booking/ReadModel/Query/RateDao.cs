using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class RateDao : IRateDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public RateDao(Func<BookingDbContext> contextFactory)
        {            
            _contextFactory = contextFactory;
        }

        public IList<RateDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<RateDetail>().ToList();
            }
        }
    }
}