#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PushNotificationAdministrationServiceClient : BaseServiceClient
    {
        public PushNotificationAdministrationServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
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