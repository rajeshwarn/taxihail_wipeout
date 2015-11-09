using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using RestSharp.Extensions;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class DispatcherSettingsDao : IDispatcherSettingsDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public DispatcherSettingsDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public DispatcherSettingsDetail GetSettings(string market)
        {
            if (!market.HasValue())
            {
                market = null;
            }

            using (var context = _contextFactory.Invoke())
            {
                return context.Query<DispatcherSettingsDetail>().SingleOrDefault(c => c.Market == market);
            }
        }
    }
}