using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;

namespace MK.Booking.Test.OrderStatusUpdate
{
    public class FakeOrderStatusUpdater : OrderStatusUpdater
    {
        public FakeOrderStatusUpdater(IServerSettings serverSettings)
            : base(serverSettings, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)
        {
        }

        public override Task HandleManualRidelinqFlow(OrderStatusDetail orderStatusDetail)
        {
            return Task.Delay(120);
        }

        public override async Task Update(IBSOrderInformation orderFromIbs, OrderStatusDetail orderStatusDetail)
        {
            //eHail Order
            await Task.Delay(500);

            //Call Geo
            await Task.Delay(500);

            //Process Geo
            await Task.Delay(500);
        }
    }

    public class FakeOrderStatusUpdateDao : IOrderStatusUpdateDao
    {
        public OrderStatusUpdateDetail GetLastUpdate()
        {
            return null;
        }

        public void UpdateLastUpdate(string updaterUniqueId, DateTime updateTime, DateTime? cycleStartTime)
        {
        }
    }
}
