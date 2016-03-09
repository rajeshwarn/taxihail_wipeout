#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Http.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PushNotificationAdministrationServiceClient : BaseServiceClient
    {
        public PushNotificationAdministrationServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService, null)
        {
        }

        public Task SendManualPushNotification(string emailAddress, string message)
        {
            return Client.Post("/admin/pushnotifications/" + Uri.EscapeDataString(emailAddress),
                new PushNotificationAdministrationRequest
                {
                    Message = message
                });
        }
    }
}