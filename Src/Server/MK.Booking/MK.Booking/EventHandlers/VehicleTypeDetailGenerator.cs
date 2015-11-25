using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class VehicleTypeDetailGenerator :
        IEventHandler<VehicleTypeAddedUpdated>,
        IEventHandler<VehicleTypeDeleted>
    {
        private readonly IProjectionSet<VehicleTypeDetail> _vehicleProjectionSet;

        public VehicleTypeDetailGenerator(IProjectionSet<VehicleTypeDetail> vehicleProjectionSet)
        {
            _vehicleProjectionSet = vehicleProjectionSet;
        }

        public void Handle(VehicleTypeAddedUpdated @event)
        {
            if (!_vehicleProjectionSet.Exists(@event.VehicleTypeId))
            {
                _vehicleProjectionSet.Add(new VehicleTypeDetail
                {
                    Id = @event.VehicleTypeId,
                    CreatedDate = @event.EventDate
                });
            }

            _vehicleProjectionSet.Update(@event.VehicleTypeId, vehicle =>
            {
                vehicle.Name = @event.Name;
                vehicle.LogoName = @event.LogoName;
                vehicle.ReferenceDataVehicleId = @event.ReferenceDataVehicleId;
                vehicle.MaxNumberPassengers = @event.MaxNumberPassengers;
                vehicle.ReferenceNetworkVehicleTypeId = @event.ReferenceNetworkVehicleTypeId;
                vehicle.IsWheelchairAccessible = @event.IsWheelchairAccessible;
            });
        }

        public void Handle(VehicleTypeDeleted @event)
        {
            _vehicleProjectionSet.Remove(@event.VehicleTypeId);
        }
    }
}