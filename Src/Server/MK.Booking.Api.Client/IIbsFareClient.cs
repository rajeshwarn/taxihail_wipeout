#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IIbsFareClient
    {
        Task<DirectionInfo> GetDirectionInfoFromIbs(double pickupLatitude, double pickupLongitude,
            double dropoffLatitude, double dropoffLongitude, 
			string pickupZipCode, string dropoffZipCode,
			string accountNumber, int? duration, int? vehicleType);

        Task<DirectionInfo> GetDirectionInfoFromDistance(double? distance, int? waitTime, 
             int? stopCount, int? passengerCount, 
             int? vehicleType, int defaultVehiculeTypeId, 
             string accountNumber, int? customerNumber);
    }
}