#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PushNotificationRegistrationServiceClient : BaseServiceClient
    {
        public PushNotificationRegistrationServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

        public Task Register(string deviceToken, PushNotificationServicePlatform platform)
        {
            return Client.PostAsync<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken),
                new PushNotificationRegistration
                {
                    Platform = platform
                }, logger: Logger);
        }

        public Task Unregister(string deviceToken)
        {
            return Client.DeleteAsync<string>("account/pushnotifications/" + Uri.EscapeDataString(deviceToken), logger: Logger);
        }

        public Task Replace(string oldDeviceToken, string newDeviceToken, PushNotificationServicePlatform platform)
        {
            return Client.PostAsync<string>("account/pushnotifications/" + Uri.EscapeDataString(newDeviceToken),
                new PushNotificationRegistration
                {
                    OldDeviceToken = oldDeviceToken,
                    Platform = platform
                }, logger: Logger);
        }
    }
}