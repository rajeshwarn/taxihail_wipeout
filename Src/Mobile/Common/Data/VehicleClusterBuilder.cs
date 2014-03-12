using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Data
{
	public class VehicleClusterBuilder
    {
        private readonly List<AvailableVehicle> _vehicles = new List<AvailableVehicle>();
        public void Add(AvailableVehicle vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException();

            _vehicles.Add(vehicle);
        }

        public bool IsEmpty { get { return _vehicles.Count == 0; } }

        public AvailableVehicle Build()
        {
            return new AvailableVehicleCluster
            {
                Latitude = IsEmpty
                    ? default(double)
				    : _vehicles.Sum(x => x.Latitude) / _vehicles.Count,
                Longitude = IsEmpty
                    ? default(double)
                    : _vehicles.Sum(x => x.Longitude) / _vehicles.Count,
            };
        }
	}
}