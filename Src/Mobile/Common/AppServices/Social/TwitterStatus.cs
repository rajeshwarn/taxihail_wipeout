using System;

namespace apcurium.MK.Booking.Mobile.AppServices.Social
{
	public class TwitterStatus : EventArgs
	{
		public bool IsConnected { get; set; }

		public TwitterStatus (bool isConnected)
		{
			IsConnected = isConnected;
		}
	}
}

