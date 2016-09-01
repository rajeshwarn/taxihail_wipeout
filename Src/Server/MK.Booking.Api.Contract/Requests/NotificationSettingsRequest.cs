using System;
using MK.Common.Configuration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/settings/notifications", "GET, POST")]
    [Route("/settings/notifications/{AccountId}", "GET, POST")]
    public class NotificationSettingsRequest
    {
        public Guid? AccountId { get; set; }
        public NotificationSettings NotificationSettings { get; set; }
    }
}