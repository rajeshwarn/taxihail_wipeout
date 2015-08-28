#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class AirlineDao : IAirlineDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AirlineDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<Airline> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Airline>().ToList();
            }
        }

        public Airline FindById(string id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Airline>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<Airline> FindByName(string text)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Airline>().Where(c => c.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
            }
        }

    }
}