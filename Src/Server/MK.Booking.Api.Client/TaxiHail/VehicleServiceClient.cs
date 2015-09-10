using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using System;

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

		public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int? vehicleTypeId)
		{
			var response = await  Client.PostAsync(new AvailableVehicles
				{
					Latitude = latitude,
					Longitude = longitude,
					VehicleTypeId = vehicleTypeId
				});

			_logger.Maybe (() => _logger.LogMessage (string.Format ("Available vehicle found for lat {0}, long {1}, count = {2}", latitude, longitude, response.Count)));

			return response.ToArray();
		}

		public Task<AvailableVehicle> GetTaxiLocation(Guid orderId, string medallion)
		{
			return Client.GetAsync(new TaxiLocationRequest
			{
				Medallion = medallion,
				OrderId = orderId
			});
		}

	    public Task<EtaForPickupResponse> GetEtaFromGeo(double latitude, double longitude, string vehicleRegistration, Guid orderId)
	    {
	        return Client.PostAsync(new EtaForPickupRequest
	        {
	            Longitude = longitude,
	            Latitude = latitude,
	            VehicleRegistration = vehicleRegistration,
	            OrderId = orderId
	        });
	    }

		public async Task<VehicleType[]> GetVehicleTypes()
	    {
            var response = await Client.GetAsync<VehicleType[]>("/admin/vehicletypes");

            return response.ToArray();
	    }

	    public async Task SendMessageToDriver(string message, string vehicleNumber)
	    {
            var request = string.Format("/vehicle/{0}/message", vehicleNumber);

            await Client.PostAsync<string>(request,
                new SendMessageToDriverRequest
	            {
	                Message = message,
	                VehicleNumber = vehicleNumber
	            });
	    }
    }
}
