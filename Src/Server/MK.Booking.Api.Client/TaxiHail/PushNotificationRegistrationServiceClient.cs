#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Enumeration;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PushNotificationRegistrationServiceClient : BaseServiceClient
    {
        public PushNotificationRegistrationServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }

        public Task Register(string deviceToken, PushNotificationServicePlatform platform)
        {
            return Client.PostAsync<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken),
                new PushNotificationRegistration
                {
                    Platform = platform
                });
        }

        public Task Unregister(string deviceToken)
        {
            return Client.DeleteAsync<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken));
        }

        public Task Replace(string oldDeviceToken, string newDeviceToken, PushNotificationServicePlatform platform)
        {
            return Client.PostAsync<string>("account/pushnotifications/" + Uri.EscapeDataString(newDeviceToken),
                new PushNotificationRegistration
                {
                    OldDeviceToken = oldDeviceToken,
                    Platform = platform
                });
        }
    }
}