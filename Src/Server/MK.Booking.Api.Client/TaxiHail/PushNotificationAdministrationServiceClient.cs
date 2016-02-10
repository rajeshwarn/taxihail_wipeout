#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PushNotificationAdministrationServiceClient : BaseServiceClient
    {
        public PushNotificationAdministrationServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService, null)
        {
        }

        public void SendManualPushNotification(string emailAddress, string message)
        {
            Client.Post<string>("/admin/pushnotifications/" + Uri.EscapeDataString(emailAddress),
                new PushNotificationAdministrationRequest
                {
                    Message = message
                });
        }
    }
}