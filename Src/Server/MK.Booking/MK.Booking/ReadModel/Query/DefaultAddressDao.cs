using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class DefaultAddressDao : IDefaultAddressDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public DefaultAddressDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<DefaultAddressDetails> GetAll()
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<DefaultAddressDetails>().ToList();
            }
        }

        public DefaultAddressDetails FindById(Guid id)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<DefaultAddressDetails>().SingleOrDefault(c => c.Id == id);
            }
        }
    }
}