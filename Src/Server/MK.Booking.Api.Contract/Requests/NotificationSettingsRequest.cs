using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/settings/notifications", "GET, POST")]
    [RouteDescription("/settings/notifications/{AccountId}", "GET, POST")]
    public class NotificationSettingsRequest
    {
        public Guid? AccountId { get; set; }
        public NotificationSettings NotificationSettings { get; set; }
    }
}