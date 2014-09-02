using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;

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
                    
			_logger.Maybe(() => _logger.LogMessage (string.Format("Available vehicle found for lat {0}, long {1} count = {2}",latitude,longitude, response.Count )));

			// Fake cars

			double centerLatitude = 45.495;
			double centerLongitude = -73.641572;

			for (float i = 0; i < 30; i++) {
				var theta = 360f * (i / 16f);
				var angle = System.Math.PI * theta / 180.0;
				var amplitude = (i * 4) * 0.00030;
				var offsetLat = System.Math.Cos (angle) * amplitude;
				var offsetLng = System.Math.Sin (angle) * amplitude;

				response.Add(new AvailableVehicle() { Latitude = centerLatitude + offsetLat, Longitude = centerLongitude + offsetLng, LogoName = "taxi", VehicleNumber = i + 1 });
			}
			return response.ToArray();
        }

		public async Task<VehicleType[]> GetVehicleTypes()
	    {
            var response = await Client.GetAsync<VehicleType[]>("/admin/vehicletypes");

            return response.ToArray();
	    }
    }
}
