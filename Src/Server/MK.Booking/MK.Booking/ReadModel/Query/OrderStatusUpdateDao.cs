#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class OrderStatusUpdateDao : IOrderStatusUpdateDao
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly Guid _defaultId = new Guid("7A6A2C45-0282-46E3-8657-0D314BED004A");
        public OrderStatusUpdateDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public OrderStatusUpdateDetail GetLastUpdate()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderStatusUpdateDetail>().SingleOrDefault(s => s.Id == _defaultId);
            }
        }

        public void UpdateLastUpdate(string updaterUniqueId, DateTime updateTime, DateTime? cycleStartTime)
        {
            using (var context = _contextFactory.Invoke())
            {
                var lastUpdate =  context.Query<OrderStatusUpdateDetail>().FirstOrDefault() ?? new OrderStatusUpdateDetail();
                lastUpdate.Id = _defaultId;
                lastUpdate.UpdaterUniqueId = updaterUniqueId;
                lastUpdate.LastUpdateDate = updateTime;
                lastUpdate.CycleStartDate = cycleStartTime;
                
                context.Save(lastUpdate);
            }
        }
 
    }
}