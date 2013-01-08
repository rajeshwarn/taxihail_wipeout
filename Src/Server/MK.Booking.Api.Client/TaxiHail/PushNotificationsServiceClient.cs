using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace MK.Booking.Api.Client.Android
{
	public class PushNotificationsServiceClient: BaseServiceClient
	{
		public PushNotificationsServiceClient(string url, string sessionId)
			: base(url, sessionId)
		{
		}
			
		public void Register(string registrationId)
		{
			var result = Client.Post<string>("/pushnotifications/" + Uri.EscapeDataString(registrationId), null);
		}

		public void Unregister(string registrationId)
		{
			Client.Delete<string>("/pushnotifications/" + Uri.EscapeDataString(registrationId));
		}
	}
}

