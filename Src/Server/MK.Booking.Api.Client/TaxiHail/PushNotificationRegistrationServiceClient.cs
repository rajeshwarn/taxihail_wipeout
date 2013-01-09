using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class PushNotificationRegistrationServiceClient: BaseServiceClient
	{
		public PushNotificationRegistrationServiceClient(string url, string sessionId)
			: base(url, sessionId)
		{
		}
			
        public void Register(string deviceToken, PushNotificationServicePlatform platform)
		{
            Client.Post<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken), new
            {
                Platform = platform                                                                                              
            });
		}

        public void Unregister(string deviceToken)
		{
            Client.Delete<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken));
		}

        public void Replace (string oldDeviceToken, string newDeviceToken, PushNotificationServicePlatform platform)
        {
            Client.Post<string>("account/ushnotifications/" + Uri.EscapeDataString(newDeviceToken), new {
                OldDeviceToken = oldDeviceToken,
                Platform = platform                                                                                              
            });
        }
	}
}

