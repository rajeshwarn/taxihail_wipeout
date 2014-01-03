#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class TariffDao : ITariffDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public TariffDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<TariffDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<TariffDetail>().ToList();
            }
        }
    }
}