using System.Net;
using System.Threading.Tasks;
using MK.Common.Android;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class VehicleServiceClient: BaseServiceClient, IVehicleClient
    {
        public VehicleServiceClient(string url, string sessionId, ILogger logger, string userAgent)
            : base(url, sessionId,userAgent)
        {
            Logger = logger;
        }

        protected ILogger Logger
        {
            get;
            private set;
        }

        public Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude)
        {
            var tcs = new TaskCompletionSource<AvailableVehicle[]>();
			Client.GetAsync(new AvailableVehicles
            {
            	Latitude = latitude,
                Longitude = longitude
				},result => tcs.SetResult(result.ToArray()),
            (result, error) => {
                Logger.LogError(error);
                tcs.SetResult(new AvailableVehicle[0]);
            });

            return tcs.Task;

        }
    }
}
