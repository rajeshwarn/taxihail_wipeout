using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using System;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class VehicleServiceClient: BaseServiceClient, IVehicleClient
    {
        public VehicleServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

		public async Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude, int? vehicleTypeId)
		{
			var response = await  Client.PostAsync(new AvailableVehicles
				{
					Latitude = latitude,
					Longitude = longitude,
					VehicleTypeId = vehicleTypeId
				}, Logger);

			return response.ToArray();
		}

		public Task<AvailableVehicle> GetTaxiLocation(Guid orderId, string medallion)
		{
			return Client.GetAsync(new TaxiLocationRequest
			{
				Medallion = medallion,
				OrderId = orderId
            }, Logger);
		}

	    public Task<EtaForPickupResponse> GetEtaFromGeo(double latitude, double longitude, string vehicleRegistration, Guid orderId)
	    {
	        return Client.PostAsync(new EtaForPickupRequest
	        {
	            Longitude = longitude,
	            Latitude = latitude,
	            VehicleRegistration = vehicleRegistration,
	            OrderId = orderId
            }, Logger);
	    }

		public async Task<VehicleType[]> GetVehicleTypes()
	    {
            var response = await Client.GetAsync<VehicleType[]>("/admin/vehicletypes", logger: Logger);

            return response.ToArray();
	    }

	    public async Task SendMessageToDriver(string message, string vehicleNumber, Guid orderId)
	    {
            var request = string.Format("/vehicle/{0}/message", vehicleNumber);

            await Client.PostAsync<string>(request,
                new SendMessageToDriverRequest
	            {
	                Message = message,
	                VehicleNumber = vehicleNumber,
					OrderId = orderId
                }, logger: Logger);
	    }
    }
}
