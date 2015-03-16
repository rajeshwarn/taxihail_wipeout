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

		public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int? vehicleTypeId, string market = null)
		{
			var response = await  Client.PostAsync(new AvailableVehicles
				{
					Latitude = latitude,
					Longitude = longitude,
					VehicleTypeId = vehicleTypeId,
                    Market = market == string.Empty ? null : market
				});

            _logger.Maybe (() => _logger.LogMessage (string.Format ("Available vehicle found for lat {0}, long {1}, count = {2} on {3} market", 
                latitude, longitude, response.Count, market.HasValue() ? market : "local")));

			return response.ToArray();
		}

		public async Task<VehicleType[]> GetVehicleTypes()
	    {
            var response = await Client.GetAsync<VehicleType[]>("/admin/vehicletypes");

            return response.ToArray();
	    }
    }
}
