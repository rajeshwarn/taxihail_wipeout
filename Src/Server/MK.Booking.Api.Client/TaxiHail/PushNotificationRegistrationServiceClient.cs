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
			
        public void Register(string deviceToken)
		{
            Client.Post<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken), null);
		}

        public void Unregister(string deviceToken)
		{
            Client.Delete<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken));
		}

        public void Replace (string oldDeviceToken, string newDeviceToken)
        {
            Client.Post<string>("account/ushnotifications/" + Uri.EscapeDataString(newDeviceToken), new {
                OldDeviceToken = oldDeviceToken
            });
        }
	}
}

