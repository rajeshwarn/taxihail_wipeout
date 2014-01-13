using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class VehicleServiceClient: BaseServiceClient, IVehicleClient
    {
        public VehicleServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId,userAgent)
        {
        }

        public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude)
        {
            var response = await  Client.GetAsync(new AvailableVehicles
            {
            	Latitude = latitude,
                Longitude = longitude
            });
            return response.ToArray();
        }
    }
}
