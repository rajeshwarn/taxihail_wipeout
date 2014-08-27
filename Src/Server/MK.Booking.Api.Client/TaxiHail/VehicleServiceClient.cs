using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class VehicleServiceClient: BaseServiceClient, IVehicleClient
    {
		private readonly ILogger _logger;
        public VehicleServiceClient(string url, string sessionId, IPackageInfo packageInfo, ILogger logger)
            : base(url, sessionId, packageInfo)
        {
			_logger = logger;
        }

        public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int vehicleTypeId)
        {
            var response = await  Client.GetAsync(new AvailableVehicles
            {
            	Latitude = latitude,
                Longitude = longitude,
                VehicleTypeId = vehicleTypeId
            });

			if (longitude <= 2.31185506434708) {
				response.Add (new AvailableVehicle { Latitude = 48.8692532505924, Longitude = 2.30916386470199 });
			}
			_logger.LogMessage (string.Format("Available vehicle found for lat {0}, long {1} count = {2}",latitude,longitude, response.Count ));
            return response.ToArray();
        }

		public async Task<VehicleType[]> GetVehicleTypes()
	    {
            var response = await Client.GetAsync<VehicleType[]>("/admin/vehicletypes");

            return response.ToArray();
	    }
    }
}
