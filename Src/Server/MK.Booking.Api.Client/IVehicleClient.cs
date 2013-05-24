using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile
{
	public interface IVehicleClient
	{		
		bool SendMessageToDriver(string carNumber, string message);
	    AvailableVehicle[] GetAvailableVehicles(double latitude, double longitude);
	}
}

	