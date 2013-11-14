﻿using System;
using System.Linq;
using System.Collections.Generic;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class DeviceDao : IDeviceDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public DeviceDao(Func<BookingDbContext> contextFactory)
        {            
            _contextFactory = contextFactory;
        }

        public IEnumerable<DeviceDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<DeviceDetail>().ToList();
            }
        }

        public IEnumerable<DeviceDetail> FindByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<DeviceDetail>().Where(x => x.AccountId == accountId).ToList();                
            }
        }
    }
}