using System;
using System.Threading;
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
            : base(serverSettings, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)
        {
        }

        public override void HandleManualRidelinqFlow(OrderStatusDetail orderStatusDetail)
        {
            Thread.Sleep(120);
        }

        public override void Update(IBSOrderInformation orderFromIbs, OrderStatusDetail orderStatusDetail)
        {
            //eHail Order
            Thread.Sleep(500);

            //Call Geo
            Thread.Sleep(500);

            //Process Geo
            Thread.Sleep(500);
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
