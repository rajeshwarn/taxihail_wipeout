using System;

namespace apcurium.MK.Booking.Mobile
{
	public interface IVehicleClient
	{		
		void SendMessageToDriver(string message);

		bool ServerCanSendMessage ();
	}
}

