using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class VehicleTypeDetailGenerator :
        IEventHandler<VehicleTypeAddedUpdated>,
        IEventHandler<VehicleTypeDeleted>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public VehicleTypeDetailGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(VehicleTypeAddedUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var vehicleTypeDetail = context.Find<VehicleTypeDetail>(@event.VehicleTypeId);
                if (vehicleTypeDetail == null)
                {
                    vehicleTypeDetail = new VehicleTypeDetail { Id = @event.VehicleTypeId };
                    vehicleTypeDetail.CreatedDate = @event.EventDate;
                }
                
                vehicleTypeDetail.Name = @event.Name;
                vehicleTypeDetail.LogoName = @event.LogoName;
                vehicleTypeDetail.ReferenceDataVehicleId = @event.ReferenceDataVehicleId;

                context.Save(vehicleTypeDetail);
            }
        }

        public void Handle(VehicleTypeDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var existing = context.Find<VehicleTypeDetail>(@event.VehicleTypeId);
                if (existing != null)
                {
                    context.Set<VehicleTypeDetail>().Remove(existing);
                    context.SaveChanges();
                }
            }
        }
    }
}