#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

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