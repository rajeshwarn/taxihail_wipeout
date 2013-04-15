using System.Globalization;
using apcurium.MK.Common.Entity;
using MK.Common.Android;
using apcurium.MK.Booking.Mobile;

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
    }
}
