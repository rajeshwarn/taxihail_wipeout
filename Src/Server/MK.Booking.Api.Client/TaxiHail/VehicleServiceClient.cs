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
            var test = response.ToArray()[0];
            if (response.ToArray().Count() > 0)
            {
                AvailableVehicle other = new AvailableVehicle();
                other.Latitude = response.ToArray()[0].Latitude + 0.0015;
                other.Longitude = response.ToArray()[0].Longitude + 0.0015;
                other.VehicleNumber = 12;
                response.Add(other);
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
