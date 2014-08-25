
using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class NotificationsSettingsServiceClient : BaseServiceClient
    {
        public NotificationsSettingsServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<NotificationSettings> GetNotificationSettings(Guid? accountId)
        {
            const string baseRoute = "/settings/notifications";
            var req = accountId != null ? string.Format("{0}/{1}", baseRoute, accountId) : baseRoute;

            return Client.GetAsync<NotificationSettings>(req);
        }

        public Task SaveNotificationsSettings(Guid accountId, NotificationSettings notificationSettings)
        {
            var req = string.Format("/settings/notifications/{0}", accountId);
            return Client.PostAsync<string>(req, notificationSettings);
        }
    }
}