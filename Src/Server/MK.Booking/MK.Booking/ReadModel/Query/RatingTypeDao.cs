#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

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
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<RatingTypeDetail>().ToList();
            }
        }

        public RatingTypeDetail GetById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<RatingTypeDetail>().SingleOrDefault(r => r.Id == id);
            }
        }

        public RatingTypeDetail FindByName(string name, string language)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<RatingTypeDetail>().SingleOrDefault(r => r.Name == name && r.Language == language);
            }
        }
    }
}