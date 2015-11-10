using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class VehicleIdMappingGenerator : IEventHandler<VehicleIdMappingAdded>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public VehicleIdMappingGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(VehicleIdMappingAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new VehicleIdMappingDetail
                {
                    OrderId = @event.SourceId,
                    DeviceName = @event.DeviceName,
                    LegacyDispatchId = @event.LegacyDispatchId
                });
            }
        }
    }
}
