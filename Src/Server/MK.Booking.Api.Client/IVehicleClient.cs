using System;

namespace apcurium.MK.Booking.Mobile
{
	public interface IVehicleClient
	{		
		bool SendMessageToDriver(string carNumber, string message);
	}
}

