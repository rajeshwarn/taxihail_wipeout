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
    public class PickupPointDao : IPickupPointDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PickupPointDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<PickupPoint> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PickupPoint>().ToList();
            }
        }

        public PickupPoint FindById(string id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PickupPoint>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<PickupPoint> FindByName(string text)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<PickupPoint>().Where(c => c.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
            }
        }

    }
}