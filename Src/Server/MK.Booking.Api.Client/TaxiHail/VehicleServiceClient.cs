using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class VehicleServiceClient: BaseServiceClient, IVehicleClient
    {
        public VehicleServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int vehicleTypeId)
        {
            var response = await  Client.GetAsync(new AvailableVehicles
            {
            	Latitude = latitude,
                Longitude = longitude,
                VehicleTypeId = vehicleTypeId
            });
            return response.ToArray();
        }

		public async Task<VehicleType[]> GetVehicleTypes()
	    {
            var response = await Client.GetAsync<VehicleType[]>("/admin/vehicletypes");

            return response.ToArray();
	    }
    }
}
