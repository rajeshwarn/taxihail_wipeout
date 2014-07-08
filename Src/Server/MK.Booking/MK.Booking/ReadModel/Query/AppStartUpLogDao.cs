﻿using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class AppStartUpLogDao : IAppStartUpLogDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AppStartUpLogDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public AppStartUpLogDetail FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AppStartUpLogDetail>().SingleOrDefault(c => c.UserId == id);
            }
        }
    }
}
