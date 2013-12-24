#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

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
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<DefaultAddressDetails>().ToList();
            }
        }

        public DefaultAddressDetails FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<DefaultAddressDetails>().SingleOrDefault(c => c.Id == id);
            }
        }
    }
}