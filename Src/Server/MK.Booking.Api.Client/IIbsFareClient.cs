using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IIbsFareClient
    {
        Task<DirectionInfo> GetDirectionInfoFromIbs(double pickupLatitude, double pickupLongitude,
            double dropoffLatitude, double dropoffLongitude, 
			string pickupZipCode, string dropoffZipCode,
			string accountNumber, int? duration, int? vehicleType, ServiceType serviceType);
    }
}