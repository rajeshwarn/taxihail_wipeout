using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class PopularAddressDao : IPopularAddressDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PopularAddressDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<PopularAddressDetails> GetAll()
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<PopularAddressDetails>().ToList();
            }
        }

        public PopularAddressDetails FindById(Guid id)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<PopularAddressDetails>().SingleOrDefault(c => c.Id == id);
            }
        }
    }
}