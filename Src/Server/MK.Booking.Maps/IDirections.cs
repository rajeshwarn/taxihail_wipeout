#region

using System;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IDirections
    {
        Direction GetDirection(double? originLat, double? originLng, double? destinationLat, double? destinationLng, int? vehicleTypeId = null,
			DateTime? date = default(DateTime?), bool forEta = false);

        Direction GetEta(double originLat, double originLng, double destinationLat, double destinationLng);
    }
}