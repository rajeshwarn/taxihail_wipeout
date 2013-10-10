using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using MK.Common.Android;
using apcurium.MK.Booking.Mobile;
using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class VehicleServiceClient: BaseServiceClient, IVehicleClient
    {
		public VehicleServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }

		public bool SendMessageToDriver (string carNumber, string message)
		{
			return SendMessage(carNumber, message).Success;
		}

	    public SendMessageToDriverResponse SendMessage(string carNumber, string message)
        {
			var result = Client.Post(new SendMessageToDriverRequest(){
				Message = message,
				CarNumber = carNumber
			});
            return result;
        }

        public Task<AvailableVehicle[]> GetAvailableVehiclesAsync(double latitude, double longitude)
        {
            var tcs = new TaskCompletionSource<AvailableVehicle[]>();
            Client.GetAsync(new AvailableVehicles
                {
                    Latitude = latitude,
                    Longitude = longitude
                },
                result => tcs.SetResult(result.ToArray()),
                (result, error) => {
                    // TODO: Log error
                    tcs.SetResult(new AvailableVehicle[0]);
                });

            return tcs.Task;
        }
    }
}
