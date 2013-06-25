using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using MK.Common.Android;
using apcurium.MK.Booking.Mobile;
using System;

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

        public AvailableVehicle[] GetAvailableVehicles(double latitude, double longitude)
        {
            try{                           
                var x=  Client.Get(new AvailableVehicles
                {
                    Latitude = latitude,
                    Longitude = longitude
                });
                
                return x.ToArray();
            }
            catch(Exception)
            {
#warning Dirty!!
                //todo fix this
                return new AvailableVehicle[0];
            }

        }
    }
}
