#region

using System;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IDirections
    {
        Direction GetDirection(double? originLat, double? originLng, double? destinationLat, double? destinationLng, 
            string currencyPriceFormat, int? vehicleTypeId = null, DateTime? date = default(DateTime?));
    }
}