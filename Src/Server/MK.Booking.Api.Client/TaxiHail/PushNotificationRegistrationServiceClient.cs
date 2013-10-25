using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
	public class PushNotificationRegistrationServiceClient: BaseServiceClient
	{
        public PushNotificationRegistrationServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId,userAgent)
		{
		}
			
        public void Register(string deviceToken, PushNotificationServicePlatform platform)
		{
            Client.Post<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken), new PushNotificationRegistration
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
            Client.Post<string>("account/pushnotifications/" + Uri.EscapeDataString(newDeviceToken), new PushNotificationRegistration
            {
                OldDeviceToken = oldDeviceToken,
                Platform = platform                                                                                              
            });
        }


	}
}

