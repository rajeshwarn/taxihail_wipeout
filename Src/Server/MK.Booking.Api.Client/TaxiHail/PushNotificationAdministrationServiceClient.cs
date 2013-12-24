#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PushNotificationAdministrationServiceClient : BaseServiceClient
    {
        public PushNotificationAdministrationServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
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