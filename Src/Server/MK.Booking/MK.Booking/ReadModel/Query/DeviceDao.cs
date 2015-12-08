#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;

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

        public void Add(Guid accountId, string deviceToken, PushNotificationServicePlatform platform)
        {
            using (var context = _contextFactory.Invoke())
            {
                var devices = context.Set<DeviceDetail>().Where(d => d.DeviceToken == deviceToken);

                context.Set<DeviceDetail>().RemoveRange(devices.Where(d => d.AccountId != accountId));

                if (devices.None(d => d.AccountId == accountId))
                {
                    var device = new DeviceDetail
                    {
                        AccountId = accountId,
                        DeviceToken = deviceToken,
                        Platform = platform
                    };
                    context.Set<DeviceDetail>().Add(device);
                }

                context.SaveChanges();
            }
        }

        public void Remove(Guid accountId, string deviceToken)
        {
            using (var context = _contextFactory.Invoke())
            {
                var device = context.Set<DeviceDetail>().Find(accountId, deviceToken);
                if (device != null)
                {
                    context.Set<DeviceDetail>().Remove(device);
                    context.SaveChanges();
                }
            }
        }

    }
}