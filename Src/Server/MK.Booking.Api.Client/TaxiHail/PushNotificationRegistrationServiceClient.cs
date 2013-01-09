using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class PushNotificationRegistrationServiceClient: BaseServiceClient
	{
		public PushNotificationRegistrationServiceClient(string url, string sessionId)
			: base(url, sessionId)
		{
		}
			
		public void Register(string registrationId)
		{
			var result = Client.Post<string>("account/pushnotifications/" + Uri.EscapeDataString(registrationId), null);
		}

		public void Unregister(string registrationId)
		{
			Client.Delete<string>("account/pushnotifications/" + Uri.EscapeDataString(registrationId));
		}
	}
}

