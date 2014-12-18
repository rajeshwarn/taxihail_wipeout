using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Maps
{
    public interface IDirections
    {
        Direction GetDirection(double? originLat, double? originLng, double? destLat, double? destLng, 
            int? vehicleTypeId = null, DateTime? date = default(DateTime?), bool forEta = false);

        Task<Direction> GetDirectionAsync(double? originLat, double? originLng, double? destLat, double? destLng, 
            int? vehicleTypeId = null, DateTime? date = default(DateTime?), bool forEta = false);

        Direction GetEta(double originLat, double originLng, double destinationLat, double destinationLng);
    }
}